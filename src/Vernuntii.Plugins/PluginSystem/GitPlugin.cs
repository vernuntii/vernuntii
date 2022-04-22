using System.CommandLine;
using Microsoft.Extensions.Logging;
using Vernuntii.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem;

/// <summary>
/// The git plugin.
/// </summary>
public class GitPlugin : Plugin, IGitPlugin
{
    private ILogger _logger = null!;
    private NextVersionOptionsPlugin _options = null!;

    private Option<string?> _configPathOption = new Option<string?>(new[] { "--config-path", "-c" }) {
        Description = $"The configuration file path. JSON and YAML is allowed. If a directory is specified instead the configuration file" +
            $" {YamlConfigurationFileDefaults.YmlFileName}, {YamlConfigurationFileDefaults.YamlFileName} or {JsonConfigurationFileDefaults.JsonFileName}" +
            " (in each upward directory in this exact order) is searched at specified directory and above."
    };

    private Option<string?> _overridePostPreReleaseOption = new Option<string?>(new[] { "--override-post-pre-release" });

    private string? _configPath;
    private string? _overridePostPreRelease;

    /// <inheritdoc/>
    protected override void OnRegistration() =>
        PluginRegistry.TryRegister<NextVersionOptionsPlugin>();

    /// <inheritdoc/>
    protected override void OnCompletedRegistration()
    {
        PluginRegistry.First<ICommandLinePlugin>().Registered += plugin => {
            plugin.RootCommand.Add(_configPathOption);
            plugin.RootCommand.Add(_overridePostPreReleaseOption);
        };

        _options = PluginRegistry.First<NextVersionOptionsPlugin>().Value;
    }

    /// <inheritdoc/>
    protected override void OnSetEventAggregator()
    {
        SubscribeEvent(CommandLineEvents.ParsedCommandLineArgsEvent.Discriminator, parseResult => {
            _configPath = parseResult.GetValueForOption(_configPathOption);
            _overridePostPreRelease = parseResult.GetValueForOption(_overridePostPreReleaseOption);
        });

        SubscribeEvent(LoggingEvents.EnabledLoggingInfrastructureEvent.Discriminator, plugin => _logger = plugin.CreateLogger<GitPlugin>());

        SubscribeEvent(NextVersionEvents.CreatedGlobalServices.Discriminator, services => {
            var configuration = new ConventionalConfigurationBuilder()
                .AddConventionalYamlFileFinder()
                .AddConventionalJsonFileFinder()
                .AddFileOrFirstConventionalFile(
                    _configPath ?? Directory.GetCurrentDirectory(),
                    new[] {
                                YamlConfigurationFileDefaults.YmlFileName,
                                YamlConfigurationFileDefaults.YamlFileName,
                                JsonConfigurationFileDefaults.JsonFileName
                    },
                    out var addedFilePath,
                    configurator => configurator.UseGitDefaults())
                .Build();

            _logger.LogInformation("Use configuration file: {ConfigurationFilePath}", addedFilePath);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .UseConfigurationDefaults(configuration)));
        });

        SubscribeEvent(NextVersionEvents.CreatedCalculationServices.Discriminator, services => {
            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .UseLatestCommitVersion()
                    .UseActiveBranchCaseDefaults()
                    .UseCommitMessagesProvider()));

            if (_options.OverrideVersioningMode == null) {
                services.ConfigureVernuntii(features => features
                    .ConfigureGit(features => features
                        .UseActiveBranchCaseVersioningMode()));
            }

            if (_overridePostPreRelease != null) {
                services.ConfigureVernuntii(features => features
                    .ConfigureGit(features => features
                        .ConfigurePreRelease(configurer => configurer
                            .SetPostPreRelease(_overridePostPreRelease))));
            }
        });
    }
}
