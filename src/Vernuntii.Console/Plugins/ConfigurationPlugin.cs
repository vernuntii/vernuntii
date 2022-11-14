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

        ///// <summary>
        ///// Constructs this type.
        ///// </summary>
        ///// <param name="sharedOptions"></param>
        ///// <param name="logging"></param>
        ///// <param name="cacheCheckPlugin"></param>
        ///// <exception cref="ArgumentNullException"></exception>
        //public ConfigurationPlugin(
        //    SharedOptionsPlugin sharedOptions,
        //    ILoggingPlugin logging,
        //    IVersionCacheCheckPlugin cacheCheckPlugin)
        //{
        //    //_sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        //    //ArgumentNullException.ThrowIfNull(logging);
        //    //_logger = logging.CreateLogger<ConfigurationPlugin>();
        //    //_cacheCheckPlugin = cacheCheckPlugin ?? throw new ArgumentNullException(nameof(sharedOptions));
        //}

        //protected override void OnRegistration(RegistrationContext registrationContext)
        //{
        //     registrationContext.PluginRegistry.re<SharedOptionsPlugin>();
        //    registrationContext.
        //}

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

            Events.SubscribeOnce(SharedOptionsEvents.ParsedCommandLineArgs, () => {
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
                Events.Publish(ConfigurationEvents.ConfiguredConfigurationBuilder);
            });

            Events.SubscribeOnce(
                ConfigurationEvents.CreateConfiguration,
                () => {
                    var configPathOrCurrentDirectory = _sharedOptions.ConfigPath ?? Directory.GetCurrentDirectory();
                    Configuration = configurationBuilder.Build();
                    _logger.LogInformation("Use configuration file: {ConfigurationFilePath}", _sharedOptions.ConfigPath);
                    Events.Publish(ConfigurationEvents.CreatedConfiguration, Configuration);
                },
                () => !versionCacheCheckPlugin.IsCacheUpToDate);
        }
    }
}
