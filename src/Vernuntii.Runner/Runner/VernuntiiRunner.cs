using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Runner
{
    /// <summary>
    /// Represents the main entry point to calculate the next version.
    /// </summary>
    public sealed class VernuntiiRunner : IVernuntiiRunner
    {
        /// <inheritdoc/>
        public IEventSystem PluginEvents => _pluginEvents;

        /// <inheritdoc/>
        public IPluginRegistry Plugins => _pluginRegistry;

        /// <inheritdoc/>
        public string[] ConsoleArguments {
            get => _args;
            init => _args = value ?? throw new ArgumentNullException(nameof(value));
        }

        private bool _isDisposed;
        private readonly PluginRegistry _pluginRegistry;
        private EventSystem _pluginEvents;
        private PluginExecutor? _pluginExecutor;
        private readonly ILogger _logger;
        private string[] _args = Array.Empty<string>();
        private SemaphoreSlim _lifecycleLock;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        internal VernuntiiRunner(PluginRegistryBuilder pluginRegistryBuilder, PluginRegistrar pluginRegistrar)
        {
            _lifecycleLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
            pluginRegistryBuilder.ConfigurePluginServices(services => services.AddSingleton(this));
            _pluginRegistry = pluginRegistryBuilder.Build(pluginRegistrar);
            var loggingPlugin = _pluginRegistry.GetPlugin<ILoggingPlugin>();
            _logger = loggingPlugin.CreateLogger<VernuntiiRunner>();

            MeasureEventSystemPerformance(loggingPlugin.CreateLogger<EventSystem>());
            _pluginEvents ??= new EventSystem();
        }

        [Conditional("DEBUG")]
        private void MeasureEventSystemPerformance(ILogger<EventSystem> logger) =>
            _pluginEvents = new PerformanceMeasuringEventSystem(logger);

        private bool _alreadyInitiatedLifecycleOnce;

        private void EnsureNotDisposed()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException("The Vernuntii runner has been already disposed");
            }
        }

        [MemberNotNullWhen(true, nameof(_pluginExecutor))]
        private bool ChackHavingPluginExecutor()
        {
            EnsureNotDisposed();
            return _pluginExecutor is not null;
        }

        [MemberNotNull(nameof(_pluginExecutor))]
        private async Task EnsureExecutingPluginOnce()
        {
            if (_pluginExecutor is not null) {
                return;
            }

            _pluginExecutor = new PluginExecutor(_pluginRegistry, _pluginEvents);
            _logger.LogTrace("Execute plugins");
            await _pluginExecutor.ExecuteAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Prepares the program run.
        /// </summary>
        /// <returns>
        /// A short-circuit exit code indicating to short-circuit the program.
        /// </returns>
        [MemberNotNull(nameof(_pluginExecutor))]
        private async Task<(LifecycleEvents.LifecycleContext LifecycleContext, ExitCode? ShortCircuitExitCode)> BeginLifecycleAsync(bool preferExceptionOverExitCode)
        {
            await EnsureExecutingPluginOnce().ConfigureAwait(false);

            var commandLineLifecycleContext = default(CommandLineEvents.LifecycleContext);
            using var commandLineOnBeforeEveryRunSubscription = _pluginEvents.OneTime(CommandLineEvents.OnBeforeEveryRun).Subscribe(context => {
                commandLineLifecycleContext = context;
            });

            var lifecycleContext = new LifecycleEvents.LifecycleContext();
            await DistinguishableEventEmitter.EmitAsync(_pluginEvents, LifecycleEvents.BeforeEveryRun, lifecycleContext).ConfigureAwait(false);

            if (commandLineLifecycleContext is null) {
                throw new NotImplementedException("The command-line lifecycle event was not fired, which may indicate that the command-line plugin was configured incorrectly");
            }
            commandLineLifecycleContext.PreferExceptionOverExitCode = preferExceptionOverExitCode;

            if (_alreadyInitiatedLifecycleOnce) {
                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, LifecycleEvents.BeforeNextRun, lifecycleContext).ConfigureAwait(false);
            }

            if (!_alreadyInitiatedLifecycleOnce) {
                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, CommandLineEvents.SetCommandLineArguments, ConsoleArguments).ConfigureAwait(false);

                var commandLineArgumentsParsingContext = new CommandLineArgumentsParsingContext();
                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, CommandLineEvents.ParseCommandLineArguments, commandLineArgumentsParsingContext).ConfigureAwait(false);

                if (commandLineLifecycleContext.ExitCode.HasValue) {
                    return (lifecycleContext, (ExitCode)commandLineLifecycleContext.ExitCode.Value);
                }

                if (!commandLineArgumentsParsingContext.HasParseResult) {
                    _logger.LogError("The command-line parse result is unexpectedly null");
                    return (lifecycleContext, ExitCode.Failure);
                }

                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, CommandLineEvents.ParsedCommandLineArguments, commandLineArgumentsParsingContext.ParseResult).ConfigureAwait(false);
                await _pluginEvents.EmitAsync(LoggingEvents.EnableLoggingInfrastructure).ConfigureAwait(false);
            }

            _alreadyInitiatedLifecycleOnce = true;
            return (lifecycleContext, null);
        }

        private async Task<int> RunAsyncCore()
        {
            var exitCode = (int)ExitCode.NotExecuted;
            using var exitCodeSubscription = _pluginEvents.OneTime(CommandLineEvents.InvokedRootCommand).Subscribe(i => exitCode = i);

            await _pluginEvents.EmitAsync(CommandLineEvents.InvokeRootCommand).ConfigureAwait(false);

            if (exitCode == (int)ExitCode.NotExecuted) {
                throw new InvalidOperationException("The Vernuntii runner was not executed");
            }

            await _pluginEvents.EmitAsync(LifecycleEvents.EndOfRun);
            return exitCode;
        }

        /// <inheritdoc/>
        internal async Task<int> RunAsync(bool preferExceptionOverExitCode)
        {
            await _lifecycleLock.WaitAsync();
            LifecycleEvents.LifecycleContext lifecycleContext;
            int exitCode;

            try {
                EnsureNotDisposed();
                (lifecycleContext, var shortCircuitExitCode) = await BeginLifecycleAsync(preferExceptionOverExitCode).ConfigureAwait(false);

                if (shortCircuitExitCode.HasValue) {
                    return (int)shortCircuitExitCode.Value;
                }

                exitCode = await RunAsyncCore().ConfigureAwait(false);
            } finally {
                _lifecycleLock.Release();
            }

            await lifecycleContext.WaitForBackgroundTasks().ConfigureAwait(false);
            return exitCode;
        }

        /// <inheritdoc/>
        public Task<int> RunAsync() =>
            RunAsync(preferExceptionOverExitCode: false);

        /// <inheritdoc/>
        public async Task<NextVersionResult> NextVersionAsync()
        {
            await _lifecycleLock.WaitAsync();
            LifecycleEvents.LifecycleContext lifecycleContext;
            NextVersionResult? nextVersionResult = null;

            try {
                EnsureNotDisposed();
                (lifecycleContext, _) = await BeginLifecycleAsync(preferExceptionOverExitCode: true).ConfigureAwait(false);

                using var subscription = _pluginEvents
                    .OneTime(NextVersionEvents.OnCalculatedNextVersion)
                    .Subscribe(result => nextVersionResult = result);

                _ = await RunAsyncCore().ConfigureAwait(false);

                if (nextVersionResult is null) {
                    throw new InvalidOperationException("The next version was not calculated");
                }
            } finally {
                _lifecycleLock.Release();
            }

            await lifecycleContext.WaitForBackgroundTasks().ConfigureAwait(false);
            return nextVersionResult;
        }

        private async Task DestroyPluginsAsync()
        {
            if (!ChackHavingPluginExecutor()) {
                return;
            }

            await _pluginExecutor.DestroyAsync(_logger).ConfigureAwait(false);
            // Which Includes the logging plugin, so new log messages are not written to console
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();
            await DestroyPluginsAsync().ConfigureAwait(false);

            if (_pluginRegistry is not null) {
                await _pluginRegistry.DisposeAsync().ConfigureAwait(false);
            }

            _lifecycleLock.Dispose();
            _isDisposed = true;
        }
    }
}

