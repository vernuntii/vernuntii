using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Vernuntii.CommandLine;
using Vernuntii.Lifecycle;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.VersionCaching;

namespace Vernuntii.Console
{
    /// <summary>
    /// Represents the main entry point to calculate the next version.
    /// </summary>
    public sealed class VernuntiiRunner : IAsyncDisposable
    {
        /// <summary>
        /// The console arguments.
        /// </summary>
        public string[] ConsoleArguments {
            get => _args;
            init => _args = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Additional plugins.
        /// </summary>
        public IEnumerable<PluginDescriptor>? PluginDescriptors { get; init; }

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
        internal VernuntiiRunner(PluginRegistry pluginRegistry)
        {
            _pluginRegistry = pluginRegistry;
            var loggingPlugin = pluginRegistry.GetPlugin<ILoggingPlugin>();
            _logger = loggingPlugin.CreateLogger<VernuntiiRunner>();

#if DEBUG
            _pluginEvents = new PerformanceMeasuringEventSystem(loggingPlugin.CreateLogger<EventSystem>());
#endif
        }

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
        private async ValueTask EnsureHavingOperablePlugins()
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
        private async ValueTask<ExitCode?> InitiateLifecycleAsync()
        {
            await EnsureHavingOperablePlugins().ConfigureAwait(true);
            _lifecycleContext = new LifecycleContext();
            await _pluginEvents.FulfillAsync(LifecycleEvents.BeforeEveryRun, _lifecycleContext).ConfigureAwait(true);

            if (!_alreadyInitiatedLifecycleOnce) {
                _logger.LogTrace("Set command-line arguments");
                await _pluginEvents.FulfillAsync(CommandLineEvents.SetCommandLineArguments, ConsoleArguments).ConfigureAwait(true);

                var commandLineArgumentsParsingContext = new CommandLineArgumentsParsingContext();
                await _pluginEvents.FulfillAsync(CommandLineEvents.ParseCommandLineArguments, commandLineArgumentsParsingContext).ConfigureAwait(true);

                if (_lifecycleContext.ExitCode.HasValue) {
                    return (ExitCode)_lifecycleContext.ExitCode.Value;
                }

                if (!commandLineArgumentsParsingContext.HasParseResult) {
                    _logger.LogError("The command-line parse result is unexpectedly null");
                    return ExitCode.Failure;
                }

                await _pluginEvents.FulfillAsync(CommandLineEvents.ParsedCommandLineArguments, commandLineArgumentsParsingContext.ParseResult).ConfigureAwait(true);
                await _pluginEvents.FulfillAsync(ConfigurationEvents.CreateConfiguration).ConfigureAwait(true);
                await _pluginEvents.FulfillAsync(ServicesEvents.CreateServiceProvider).ConfigureAwait(true);
                await _pluginEvents.FulfillAsync(LoggingEvents.EnableLoggingInfrastructure).ConfigureAwait(true);
            }

            if (_alreadyInitiatedLifecycleOnce) {
                await _pluginEvents.FulfillAsync(LifecycleEvents.BeforeNextRun, _lifecycleContext).ConfigureAwait(true);
                await _pluginEvents.FulfillAsync(VersionCacheCheckEvents.CheckVersionCache).ConfigureAwait(true);
            }

            _alreadyInitiatedLifecycleOnce = true;
            return null;
        }

        private async ValueTask<int> RunAsyncCore()
        {
            EnsureHavingPluginEvents();

            var exitCode = (int)ExitCode.NotExecuted;
            using var exitCodeSubscription = _pluginEvents.Earliest(CommandLineEvents.InvokedRootCommand).Subscribe(i => exitCode = i);

            _logger.LogTrace("Invoke command-line root command");
            await _pluginEvents.FulfillAsync(CommandLineEvents.InvokeRootCommand).ConfigureAwait(true);

            if (exitCode == (int)ExitCode.NotExecuted) {
                throw new InvalidOperationException("The Vernuntii runner was not executed");
            }

            return exitCode;
        }

        /// <summary>
        /// Runs Vernuntii for console.
        /// </summary>
        /// <returns>
        /// The promise of an exit code.
        /// </returns>
        public async ValueTask<int> RunAsync()
        {
            EnsureNotDisposed();
            await EnsureHavingOperablePlugins().ConfigureAwait(true);
            _pluginRegistry.GetPlugin<ICommandLinePlugin>().PreferExceptionOverExitCode = false;
            var shortCircuitExitCode = await InitiateLifecycleAsync().ConfigureAwait(true);

            if (shortCircuitExitCode.HasValue) {
                return (int)shortCircuitExitCode.Value;
            }

            return await RunAsyncCore().ConfigureAwait(true);
        }

        /// <summary>
        /// Runs Vernuntii for getting the next version. The presence of <see cref="INextVersionPlugin"/> and its dependencies is expected.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public async ValueTask<IVersionCache> NextVersionAsync()
        {
            EnsureNotDisposed();
            await EnsureHavingOperablePlugins().ConfigureAwait(true);
            _pluginRegistry.GetPlugin<ICommandLinePlugin>().PreferExceptionOverExitCode = true;
            _ = await InitiateLifecycleAsync().ConfigureAwait(true);
            IVersionCache? versionCache = null;

            using var subscription = _pluginEvents
                .Earliest(NextVersionEvents.CalculatedNextVersion)
                .Subscribe(calculatedVersionCache => versionCache = calculatedVersionCache);

            _ = await RunAsyncCore().ConfigureAwait(true);

            if (versionCache is null) {
                throw new InvalidOperationException("The next version was not calculated");
            }

            return versionCache;
        }

        private async ValueTask DestroyPluginsAsync()
        {
            if (!ChackHavingPluginExecutor()) {
                return;
            }

            _logger.LogTrace("Destroying plugins");
            await _pluginExecutor.DestroyAsync().ConfigureAwait(true);
            _logger.LogTrace("Destroyed plugins");
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();
            await DestroyPluginsAsync().ConfigureAwait(true);

            if (_pluginRegistry is not null) {
                await _pluginRegistry.DisposeAsync().ConfigureAwait(true);
            }

            _isDisposed = true;
        }
    }
}

