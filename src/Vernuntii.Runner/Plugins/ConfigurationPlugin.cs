using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vernuntii.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Extensions;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The plugin provides a global <see cref="IConfiguration"/> instance.
    /// </summary>
    [Plugin(Order = -1000)]
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
        private readonly ICommandLinePlugin _commandLinePlugin;
        private readonly ILogger _logger;
        private bool _isConfigurationBuilderConfigured;
        private string? _configFile;

        public ConfigurationPlugin(IPluginRegistry pluginRegistry, ICommandLinePlugin commandLinePlugin, ILogger<ConfigurationPlugin> logger)
        {
            _pluginRegistry = pluginRegistry ?? throw new ArgumentNullException(nameof(pluginRegistry));
            _commandLinePlugin = commandLinePlugin;
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
            Option<string?> configPathOption = new(new[] { "--config-path", "-c" }) {
                Description = $"The configuration file path. JSON and YAML is allowed. If a directory is specified instead the configuration file" +
                $" {YamlConfigurationFileDefaults.YmlFileName}, {YamlConfigurationFileDefaults.YamlFileName} or {JsonConfigurationFileDefaults.JsonFileName}" +
                " (in each upward directory in this exact order) is searched at specified directory and above."
            };

            _commandLinePlugin.RootCommand.Add(configPathOption);

            var configurationBuilder = new ConventionalConfigurationBuilder()
                .AddConventionalYamlFileFinder()
                .AddConventionalJsonFileFinder();

            Events.OneTime(CommandLineEvents.ParsedCommandLineArguments)
                .Subscribe(async parseResult => {
                    var configPath = parseResult.GetValueForOption(configPathOption) ?? Directory.GetCurrentDirectory();
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

                    _isConfigurationBuilderConfigured = true;

                    await Events.EmitAsync(
                        ConfigurationEvents.OnConfiguredConfigurationBuilder,
                        new ConfigurationEvents.ConfiguredConfigurationBuilderResult() { ConfigPath = addedFilePath }).ConfigureAwait(false);
                });

            Events.OneTime(ConfigurationEvents.CreateConfiguration)
                .And(ConfigurationEvents.OnConfiguredConfigurationBuilder)
                .Subscribe(result => {
                    if (_pluginRegistry.TryGetPlugin<IVersionCachePlugin>(out var versionCacheChecker) && versionCacheChecker.IsCacheUpToDate) {
                        return Task.CompletedTask;
                    }

                    var (_, builderResult) = result;
                    Configuration = configurationBuilder.Build();
                    _logger.LogInformation("Used configuration file: {ConfigurationFilePath}", builderResult.ConfigPath ?? "<none>");
                    return Events.EmitAsync(ConfigurationEvents.OnCreatedConfiguration, Configuration);
                });
            ;
        }
    }
}
