using System.Reactive.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vernuntii.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Extensions;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The plugin provides a global <see cref="IConfiguration"/> instance.
    /// </summary>
    [Plugin(Order = -1000)]
    [ImportPlugin<SharedOptionsPlugin>(TryRegister = true)]
    public class ConfigurationPlugin : Plugin, IConfigurationPlugin
    {
        /// <inheritdoc/>
        public IConfiguration Configuration { get; private set; } = null!;

        /// <inheritdoc/>
        public string? ConfigFile {
            get {
                EnsureConfiguredConfigurationBuilder();
                return _configFile;
            }

            private set => _configFile = value;
        }

        private readonly IPluginRegistry _pluginRegistry;
        private readonly SharedOptionsPlugin _sharedOptions;
        private readonly ILogger _logger;
        private bool _isConfigurationBuilderConfigured;
        private string? _configFile;

        public ConfigurationPlugin(IPluginRegistry pluginRegistry, SharedOptionsPlugin sharedOptions, ILogger<ConfigurationPlugin> logger)
        {
            _pluginRegistry = pluginRegistry ?? throw new ArgumentNullException(nameof(pluginRegistry));
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void EnsureConfiguredConfigurationBuilder()
        {
            if (!_isConfigurationBuilderConfigured) {
                throw new InvalidOperationException("Configuration builder is not yet configured");
            }
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            var versionCacheCheckPlugin = _pluginRegistry.GetPlugin<IVersionCacheCheckPlugin>();

            var configurationBuilder = new ConventionalConfigurationBuilder()
                .AddConventionalYamlFileFinder()
                .AddConventionalJsonFileFinder();

            Events.OnNextEvent(SharedOptionsEvents.ParsedCommandLineArgs, () => {
                var configPath = _sharedOptions.ConfigPath;
                _logger.LogTrace("Search configuration file (Start = {ConfigPath})", configPath);

                // Allow non existent configuration file because defaults are applied.
                var foundConfigFile = configurationBuilder.TryAddFileOrFirstConventionalFile(
                       configPath,
                       new[] {
                           YamlConfigurationFileDefaults.YmlFileName,
                           YamlConfigurationFileDefaults.YamlFileName,
                           JsonConfigurationFileDefaults.JsonFileName
                       },
                       out var addedFilePath,
                       configurator => configurator.AddGitDefaults());

                if (foundConfigFile) {
                    _sharedOptions.ConfigPath = addedFilePath;
                }

                _isConfigurationBuilderConfigured = true;
                Events.FireEvent(ConfigurationEvents.ConfiguredConfigurationBuilder);
            });

            Events.GetEvent(ConfigurationEvents.CreateConfiguration).Take(1).Where(_ => !versionCacheCheckPlugin.IsCacheUpToDate).Subscribe(_ => {
                var configPathOrCurrentDirectory = _sharedOptions.ConfigPath ?? Directory.GetCurrentDirectory();
                Configuration = configurationBuilder.Build();
                _logger.LogInformation("Use configuration file: {ConfigurationFilePath}", _sharedOptions.ConfigPath);
                Events.FireEvent(ConfigurationEvents.CreatedConfiguration, Configuration);
            });
        }
    }
}
