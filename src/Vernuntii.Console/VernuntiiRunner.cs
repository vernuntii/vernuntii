using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Vernuntii.Plugins;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionFoundation;

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
        private PluginRegistry? _pluginRegistry;
        private PluginEventCache? _pluginEvents;
        private PluginExecutor? _pluginExecutor;
        private ILogger? _logger;
        private string[] _args = Array.Empty<string>();
        private bool _runOnce;

        private void EnsureNotDisposed()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException("The runner has been already disposed");
            }
        }

        [MemberNotNull(nameof(_logger))]
        private void EnsureHavingLogger()
        {
            if (_logger is null) {
                throw new InvalidOperationException("Logger has not been created");
            }
        }

        [MemberNotNull(nameof(_pluginRegistry))]
        private void EnsureHavingPluginRegistry()
        {
            if (_pluginRegistry is null) {
                throw new InvalidOperationException("Plugin registry has not been created");
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

        [MemberNotNull(nameof(_pluginRegistry))]
        private async ValueTask PrepareRunCoreOnceAsync()
        {
            EnsureNotDisposed();

            if (_pluginRegistry is not null) {
                return;
            }

            _pluginRegistry = new PluginRegistry();

            await _pluginRegistry.RegisterAsync<IVersioningPresetsPlugin, VersioningPresetsPlugin>();
            await _pluginRegistry.RegisterAsync<ICommandLinePlugin, CommandLinePlugin>();

            var loggerPlugin = new LoggingPlugin();
            await _pluginRegistry.RegisterAsync<ILoggingPlugin>(loggerPlugin);
            _logger = loggerPlugin.CreateLogger(nameof(VernuntiiRunner));

            await _pluginRegistry.RegisterAsync<IConfigurationPlugin, ConfigurationPlugin>();
            await _pluginRegistry.RegisterAsync<IGitPlugin, GitPlugin>();
            await _pluginRegistry.RegisterAsync<INextVersionPlugin, NextVersionPlugin>();
            await _pluginRegistry.RegisterAsync<VersionCalculationPerfomancePlugin>();

            if (PluginDescriptors is not null) {
                foreach (var pluginDescriptor in PluginDescriptors) {
                    await _pluginRegistry.RegisterAsync(pluginDescriptor.PluginType, pluginDescriptor.Plugin);
                }
            }

            _pluginEvents = new PluginEventCache();
            _pluginExecutor = new PluginExecutor(_pluginRegistry, _pluginEvents);
            await _pluginExecutor.ExecuteAsync();
            _logger.LogTrace("Executing plugins");

            _pluginEvents.Publish(CommandLineEvents.SetCommandLineArgs, ConsoleArgs);
            _logger.LogTrace("Set command-line arguments");

            _pluginEvents.Publish(CommandLineEvents.ParseCommandLineArgs);
            _logger.LogTrace("Parse command-line arguments");

            _pluginEvents.Publish(LoggingEvents.EnableLoggingInfrastructure);
            _logger.LogTrace("Enable logging infrastructure");

            _pluginEvents.Publish(ConfigurationEvents.CreateConfiguration);
            _logger.LogTrace("Create configuration");
        }

        private int RunCore()
        {
            EnsureHavingPluginEvents();
            EnsureHavingLogger();

            if (_runOnce) {
                _pluginEvents.Publish(GitEvents.UnsetRepositoryCache);
            }

            int exitCode = (int)ExitCode.NotExecuted;
            using var exitCodeSubscription = _pluginEvents.SubscribeOnce(CommandLineEvents.InvokedRootCommand, i => exitCode = i);

            _pluginEvents.Publish(CommandLineEvents.InvokeRootCommand);
            _logger.LogTrace("Invoke command-line root command");

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
            await PrepareRunCoreOnceAsync();
            return RunCore();
        }

        /// <summary>
        /// Runs Vernuntii for getting the next version.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ISemanticVersionFoundation> RunAsync()
        {
            await PrepareRunCoreOnceAsync();
            EnsureHavingPluginEvents();
            ISemanticVersionFoundation? versionFoundation = null;

            _pluginEvents.SubscribeOnce(
                NextVersionEvents.CalculatedNextVersion,
                calculatedVersionFoundation => versionFoundation = calculatedVersionFoundation);

            RunCore();

            if (versionFoundation is null) {
                throw new InvalidOperationException("Next version was not calculated");
            }

            return versionFoundation;
        }

        private async ValueTask DestroyPluginsAsync()
        {
            if (!ChackHavingPluginExecutor()) {
                return;
            }

            EnsureHavingLogger();

            _logger.LogTrace("Destroying plugins");
            await _pluginExecutor.DestroyAsync();
            _logger.LogTrace("Destroyed plugins");
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            EnsureNotDisposed();
            await DestroyPluginsAsync();
            _pluginRegistry?.Dispose();
            _isDisposed = true;
        }

        private class ExitCodeReference
        {
            public int ExitCode { get; set; }
        }
    }
}

