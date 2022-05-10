using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vernuntii.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Extensions;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
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

        private ILogger _logger = null!;

        private Option<string?> _configPathOption = new Option<string?>(new[] { "--config-path", "-c" }) {
            Description = $"The configuration file path. JSON and YAML is allowed. If a directory is specified instead the configuration file" +
                $" {YamlConfigurationFileDefaults.YmlFileName}, {YamlConfigurationFileDefaults.YamlFileName} or {JsonConfigurationFileDefaults.JsonFileName}" +
                " (in each upward directory in this exact order) is searched at specified directory and above."
        };

        private string? _configPath;

        /// <inheritdoc/>
        protected override void OnCompletedRegistration()
        {
            Plugins.First<ICommandLinePlugin>().Registered +=
                plugin => plugin.RootCommand.Add(_configPathOption);
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(
                CommandLineEvents.ParsedCommandLineArgs,
                parseResult => _configPath = parseResult.GetValueForOption(_configPathOption));

            Events.SubscribeOnce(ConfigurationEvents.CreateConfiguration, () => {
                var configurationBuilder = new ConventionalConfigurationBuilder()
                   .AddConventionalYamlFileFinder()
                   .AddConventionalJsonFileFinder();

                var configPathOrCurrentDirectory = _configPath ?? Directory.GetCurrentDirectory();
                _logger.LogTrace("Search configuration file (Start = {ConfigPath})", configPathOrCurrentDirectory);

                // Allow non existent configuration file because defaults are applied.
                _ = configurationBuilder.TryAddFileOrFirstConventionalFile(
                       configPathOrCurrentDirectory,
                       new[] {
                                YamlConfigurationFileDefaults.YmlFileName,
                                YamlConfigurationFileDefaults.YamlFileName,
                                JsonConfigurationFileDefaults.JsonFileName
                       },
                       out var addedFilePath,
                       configurator => configurator.AddGitDefaults());

                Configuration = configurationBuilder.Build();
                _logger.LogInformation("Use configuration file: {ConfigurationFilePath}", addedFilePath);
                Events.Publish(ConfigurationEvents.CreatedConfiguration, Configuration);
            });

            Events.SubscribeOnce(
                LoggingEvents.EnabledLoggingInfrastructure,
                plugin => _logger = plugin.CreateLogger<ConfigurationPlugin>());
        }
    }
}
