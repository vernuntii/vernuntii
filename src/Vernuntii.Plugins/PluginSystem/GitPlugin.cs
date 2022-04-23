using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem;

/// <summary>
/// The git plugin.
/// </summary>
public class GitPlugin : Plugin, IGitPlugin
{
    private NextVersionOptionsPlugin _options = null!;
    private IConfiguration _configuration = null!;

    private Option<string?> _overridePostPreReleaseOption = new Option<string?>(new[] { "--override-post-pre-release" });

    private string? _overridePostPreRelease;

    /// <inheritdoc/>
    protected override void OnRegistration() =>
        PluginRegistry.TryRegister<NextVersionOptionsPlugin>();

    /// <inheritdoc/>
    protected override void OnCompletedRegistration()
    {
        PluginRegistry.First<ICommandLinePlugin>().Registered += plugin => {
            plugin.RootCommand.Add(_overridePostPreReleaseOption);
        };

        _options = PluginRegistry.First<NextVersionOptionsPlugin>().Value;
    }

    /// <inheritdoc/>
    protected override void OnEventAggregator()
    {
        SubscribeEvent(
            CommandLineEvents.ParsedCommandLineArgs.Discriminator,
            parseResult => _overridePostPreRelease = parseResult.GetValueForOption(_overridePostPreReleaseOption));

        SubscribeEvent(
            ConfigurationEvents.CreatedConfiguration.Discriminator,
            configuration => _configuration = configuration);

        SubscribeEvent(NextVersionEvents.CreatedGlobalServices.Discriminator, services => {
            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .UseConfigurationDefaults(_configuration)));
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
