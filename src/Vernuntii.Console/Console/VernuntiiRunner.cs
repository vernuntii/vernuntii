using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionCaching;

namespace Vernuntii.Console
{
    /// <summary>
    /// Represents the main entry to calculate the next version.
    /// </summary>
    public sealed class VernuntiiRunner : IAsyncDisposable
    {
        /// <summary>
        /// The console arguments.
        /// </summary>
        public string[] ConsoleArgs {
            get => _args;
            init => _args = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Additional plugins.
        /// </summary>
        public IEnumerable<PluginDescriptor>? PluginDescriptors { get; init; }

        private bool _isDisposed;
        private PluginRegistry _pluginRegistry;
        private PluginEventCache? _pluginEvents;
        private PluginExecutor? _pluginExecutor;
        private ILogger _logger;
        private string[] _args = Array.Empty<string>();

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        internal VernuntiiRunner(PluginRegistry pluginRegistry)
        {
            _pluginRegistry = pluginRegistry;
            _logger = pluginRegistry.GetPlugin<ILoggingPlugin>().CreateLogger<VernuntiiRunner>();
        }

        [MemberNotNullWhen(true,
            nameof(_pluginRegistry),
            nameof(_pluginEvents),
            nameof(_pluginExecutor))]
        private bool _runOnce { get; set; }

        private void EnsureNotDisposed()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException("The runner has been already disposed");
            }
        }

        [MemberNotNull(nameof(_pluginEvents))]
        private void EnsureHavingPluginEvents()
        {
            if (_pluginEvents is null) {
                throw new InvalidOperationException("Plugin event cache has not been created");
            }
        }

        [MemberNotNullWhen(true, nameof(_pluginExecutor))]
        private bool ChackHavingPluginExecutor()
        {
            EnsureNotDisposed();
            return _pluginExecutor is not null;
        }

        /// <summary>
        /// Prepares once the program run.
        /// </summary>
        /// <returns>A short-circuit exit code indicating to short-circuit the program.</returns>
        private async ValueTask<ExitCode?> PrepareRunOnceAsync()
        {
            EnsureNotDisposed();

            if (!_runOnce) {
                if (_pluginEvents is not null || _pluginExecutor is not null) {
                    throw new InvalidOperationException("Runner was not prepared correctly");
                }

                _pluginEvents = new PluginEventCache();
                _pluginExecutor = new PluginExecutor(_pluginRegistry, _pluginEvents);
                _logger.LogTrace("Execute plugins");
                await _pluginExecutor.ExecuteAsync();
            }

            _pluginEvents.Publish(LifecycleEvents.BeforeEveryRun);

            if (!_runOnce) {
                _logger.LogTrace("Set command-line arguments");
                _pluginEvents.Publish(CommandLineEvents.SetCommandLineArgs, ConsoleArgs);

                ExitCode? shortCircuitExitCode = null;
                using var invokedRootCommandSubscripion = _pluginEvents.SubscribeOnce(CommandLineEvents.InvokedRootCommand, exitCode => shortCircuitExitCode = (ExitCode)exitCode);
                _pluginEvents.Publish(CommandLineEvents.ParseCommandLineArgs);
                _pluginEvents.Publish(LoggingEvents.EnableLoggingInfrastructure);

                if (shortCircuitExitCode.HasValue) {
                    return shortCircuitExitCode;
                }
            }

            if (_runOnce) {
                _pluginEvents.Publish(LifecycleEvents.BeforeNextRun);
                //_pluginEvents.Publish(VersionCacheCheckEvents.CheckVersionCache);
            }

            return null;
        }

        private int RunCore()
        {
            EnsureHavingPluginEvents();

            int exitCode = (int)ExitCode.NotExecuted;
            using var exitCodeSubscription = _pluginEvents.SubscribeOnce(CommandLineEvents.InvokedRootCommand, i => exitCode = i);

            _logger.LogTrace("Invoke command-line root command");
            _pluginEvents.Publish(CommandLineEvents.InvokeRootCommand);

            if (exitCode == (int)ExitCode.NotExecuted) {
                throw new InvalidOperationException("The command line was not running");
            }

            _runOnce = true;
            return exitCode;
        }

        /// <summary>
        /// Runs Vernuntii for console.
        /// </summary>
        public async Task<int> RunConsoleAsync()
        {
            var shortCircuitExitCode = await PrepareRunOnceAsync();

            if (shortCircuitExitCode.HasValue) {
                return (int)shortCircuitExitCode.Value;
            }

            return RunCore();
        }

        /// <summary>
        /// Runs Vernuntii for getting the next version.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IVersionCache> RunAsync()
        {
            await PrepareRunOnceAsync();
            EnsureHavingPluginEvents();
            IVersionCache? versionCache = null;

            _pluginEvents.SubscribeOnce(
                NextVersionEvents.CalculatedNextVersion,
                calculatedVersionCache => versionCache = calculatedVersionCache);

            RunCore();

            if (versionCache is null) {
                throw new InvalidOperationException("Next version was not calculated");
            }

            return versionCache;
        }

        private async ValueTask DestroyPluginsAsync()
        {
            if (!ChackHavingPluginExecutor()) {
                return;
            }

            _logger.LogTrace("Destroying plugins");
            await _pluginExecutor.DestroyAsync();
            _logger.LogTrace("Destroyed plugins");
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();
            await DestroyPluginsAsync();

            if (_pluginRegistry is not null) {
                await _pluginRegistry.DisposeAsync();
            }

            _isDisposed = true;
        }
    }
}

