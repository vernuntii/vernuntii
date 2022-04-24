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
            PluginRegistry.First<ICommandLinePlugin>().Registered +=
                plugin => plugin.RootCommand.Add(_configPathOption);
        }

        /// <inheritdoc/>
        protected override void OnEventAggregator()
        {
            SubscribeEvent(
                CommandLineEvents.ParsedCommandLineArgs.Discriminator,
                parseResult => _configPath = parseResult.GetValueForOption(_configPathOption));

            SubscribeEvent<ConfigurationEvents.CreateConfiguration>(() => {
                var configurationBuilder = new ConventionalConfigurationBuilder()
                   .AddConventionalYamlFileFinder()
                   .AddConventionalJsonFileFinder();

                _ = configurationBuilder.TryAddFileOrFirstConventionalFile(
                       _configPath ?? Directory.GetCurrentDirectory(),
                       new[] {
                                YamlConfigurationFileDefaults.YmlFileName,
                                YamlConfigurationFileDefaults.YamlFileName,
                                JsonConfigurationFileDefaults.JsonFileName
                       },
                       out var addedFilePath,
                       configurator => configurator.UseGitDefaults());

                Configuration = configurationBuilder.Build();
                _logger.LogInformation("Use configuration file: {ConfigurationFilePath}", addedFilePath);
                EventAggregator.PublishEvent(ConfigurationEvents.CreatedConfiguration.Discriminator, Configuration);
            });

            SubscribeEvent(
                LoggingEvents.EnabledLoggingInfrastructure.Discriminator,
                plugin => _logger = plugin.CreateLogger<ConfigurationPlugin>());
        }
    }
}
