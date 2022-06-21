using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vernuntii.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Extensions;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The plugin provides a global <see cref="IConfiguration"/> instance.
    /// </summary>
    public class ConfigurationPlugin : Plugin, IConfigurationPlugin
    {
        /// <inheritdoc/>
        public override int? Order => -1000;
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

        private SharedOptionsPlugin _sharedOptions = null!;
        private ILogger _logger = null!;
        private IVersionCacheCheckPlugin _cacheCheckPlugin = null!;
        private bool _isConfigurationBuilderConfigured;
        private string? _configFile;

        private void EnsureConfiguredConfigurationBuilder()
        {
            if (!_isConfigurationBuilderConfigured) {
                throw new InvalidOperationException("Configuration builder is not yet configured");
            }
        }

        /// <inheritdoc/>
        protected override async ValueTask OnRegistrationAsync(RegistrationContext registrationContext) =>
            await registrationContext.PluginRegistry.TryRegisterAsync<SharedOptionsPlugin>();

        /// <inheritdoc/>
        protected override void OnCompletedRegistration()
        {
            _sharedOptions = Plugins.First<SharedOptionsPlugin>();
            _logger = Plugins.First<ILoggingPlugin>().CreateLogger<ConfigurationPlugin>();
            _cacheCheckPlugin = Plugins.First<IVersionCacheCheckPlugin>();
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
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
                () => !_cacheCheckPlugin.IsCacheUpToDate);
        }
    }
}
