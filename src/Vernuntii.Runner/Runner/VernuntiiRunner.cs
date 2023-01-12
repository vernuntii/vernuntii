using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.Plugins.Events;
using Vernuntii.Plugins.Lifecycle;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.SemVer;

namespace Vernuntii.Runner
{
    /// <summary>
    /// Represents the main entry point to calculate the next version.
    /// </summary>
    public sealed class VernuntiiRunner : IVernuntiiRunner, IAsyncDisposable
    {
        /// <inheritdoc/>
        public string[] ConsoleArguments {
            get => _args;
            init => _args = value ?? throw new ArgumentNullException(nameof(value));
        }

        private bool _isDisposed;
        private readonly PluginRegistry _pluginRegistry;
        private EventSystem? _pluginEvents;
        private PluginExecutor? _pluginExecutor;
        private readonly ILogger _logger;
        private string[] _args = Array.Empty<string>();
        private LifecycleContext? _lifecycleContext;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        internal VernuntiiRunner(PluginRegistryBuilder pluginRegistryBuilder, PluginRegistrar pluginRegistrar)
        {
            pluginRegistryBuilder.ConfigurePluginServices(services => services.AddSingleton(this));
            _pluginRegistry = pluginRegistryBuilder.Build(pluginRegistrar);
            var loggingPlugin = _pluginRegistry.GetPlugin<ILoggingPlugin>();
            _logger = loggingPlugin.CreateLogger<VernuntiiRunner>();

            MeasureEventSystemPerformance(loggingPlugin.CreateLogger<EventSystem>());
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

        [MemberNotNull(nameof(_pluginEvents))]
        private void EnsureHavingPluginEvents()
        {
            if (_pluginEvents is null) {
                throw new InvalidOperationException("The Plugin event cache has not been created");
            }
        }

        [MemberNotNullWhen(true, nameof(_pluginExecutor))]
        private bool ChackHavingPluginExecutor()
        {
            EnsureNotDisposed();
            return _pluginExecutor is not null;
        }

        [MemberNotNull(nameof(_pluginEvents), nameof(_pluginExecutor))]
        private async Task EnsureHavingOperablePlugins()
        {
            if (_pluginEvents is not null && _pluginExecutor is not null) {
                return;
            }

            _pluginEvents ??= new EventSystem();
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
        private async Task<ExitCode?> InitiateLifecycleAsync()
        {
            await EnsureHavingOperablePlugins().ConfigureAwait(false);
            _lifecycleContext = new LifecycleContext();
            await DistinguishableEventEmitter.EmitAsync(_pluginEvents, LifecycleEvents.BeforeEveryRun, _lifecycleContext).ConfigureAwait(false);

            if (!_alreadyInitiatedLifecycleOnce) {
                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, CommandLineEvents.SetCommandLineArguments, ConsoleArguments).ConfigureAwait(false);

                var commandLineArgumentsParsingContext = new CommandLineArgumentsParsingContext();
                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, CommandLineEvents.ParseCommandLineArguments, commandLineArgumentsParsingContext).ConfigureAwait(false);

                if (_lifecycleContext.ExitCode.HasValue) {
                    return (ExitCode)_lifecycleContext.ExitCode.Value;
                }

                if (!commandLineArgumentsParsingContext.HasParseResult) {
                    _logger.LogError("The command-line parse result is unexpectedly null");
                    return ExitCode.Failure;
                }

                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, CommandLineEvents.ParsedCommandLineArguments, commandLineArgumentsParsingContext.ParseResult).ConfigureAwait(false);
                await _pluginEvents.EmitAsync(LoggingEvents.EnableLoggingInfrastructure).ConfigureAwait(false);
            }

            if (_alreadyInitiatedLifecycleOnce) {
                await DistinguishableEventEmitter.EmitAsync(_pluginEvents, LifecycleEvents.BeforeNextRun, _lifecycleContext).ConfigureAwait(false);
            }

            _alreadyInitiatedLifecycleOnce = true;
            return null;
        }

        private async Task<int> RunAsyncCore()
        {
            EnsureHavingPluginEvents();

            var exitCode = (int)ExitCode.NotExecuted;
            using var exitCodeSubscription = _pluginEvents.Earliest(CommandLineEvents.InvokedRootCommand).Subscribe(i => exitCode = i);

            await _pluginEvents.EmitAsync(CommandLineEvents.InvokeRootCommand).ConfigureAwait(false);

            if (exitCode == (int)ExitCode.NotExecuted) {
                throw new InvalidOperationException("The Vernuntii runner was not executed");
            }

            return exitCode;
        }

        /// <inheritdoc/>
        public async Task<int> RunAsync()
        {
            EnsureNotDisposed();
            await EnsureHavingOperablePlugins().ConfigureAwait(false);
            _pluginRegistry.GetPlugin<ICommandLinePlugin>().PreferExceptionOverExitCode = false;
            var shortCircuitExitCode = await InitiateLifecycleAsync().ConfigureAwait(false);

            if (shortCircuitExitCode.HasValue) {
                return (int)shortCircuitExitCode.Value;
            }

            return await RunAsyncCore().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<ISemanticVersion> NextVersionAsync()
        {
            EnsureNotDisposed();
            await EnsureHavingOperablePlugins().ConfigureAwait(false);
            _pluginRegistry.GetPlugin<ICommandLinePlugin>().PreferExceptionOverExitCode = true;
            _ = await InitiateLifecycleAsync().ConfigureAwait(false);
            ISemanticVersion? nextVersion = null;

            using var subscription = _pluginEvents
                .Earliest(NextVersionEvents.OnCalculatedNextVersion)
                .Subscribe(nextVersionCache => nextVersion = nextVersionCache.Version);

            _ = await RunAsyncCore().ConfigureAwait(false);

            if (nextVersion is null) {
                throw new InvalidOperationException("The next version was not calculated");
            }

            return nextVersion;
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

            _isDisposed = true;
        }
    }
}

