using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem;

/// <summary>
/// The git plugin.
/// </summary>
public class GitPlugin : Plugin, IGitPlugin
{
    private NextVersionOptionsPlugin _options = null!;
    private IConfiguration _configuration = null!;
    private INextVersionPlugin _nextVersionPlugin = null!;

    private Option<string?> _overridePostPreReleaseOption = new Option<string?>(new[] { "--override-post-pre-release" });

    private Option<bool> _duplicateVersionFailsOption = new Option<bool>(new string[] { "--duplicate-version-fails" }) {
        Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
    };

    private string? _overridePostPreRelease;
    private bool _duplicateVersionFails;

    /// <inheritdoc/>
    protected override async ValueTask OnRegistrationAsync(RegistrationContext registrationContext) =>
        await PluginRegistry.TryRegisterAsync<NextVersionOptionsPlugin>();

    /// <inheritdoc/>
    protected override void OnCompletedRegistration()
    {
        PluginRegistry.First<ICommandLinePlugin>().Registered += plugin => {
            plugin.RootCommand.Add(_overridePostPreReleaseOption);
            plugin.RootCommand.Add(_duplicateVersionFailsOption);
        };

        _options = PluginRegistry.First<NextVersionOptionsPlugin>().Value;
        _nextVersionPlugin = PluginRegistry.First<INextVersionPlugin>().Value;
    }

    /// <inheritdoc/>
    protected override void OnEventAggregation()
    {
        SubscribeEvent(
            CommandLineEvents.ParsedCommandLineArgs.Discriminator,
            parseResult => {
                _overridePostPreRelease = parseResult.GetValueForOption(_overridePostPreReleaseOption);
                _duplicateVersionFails = parseResult.GetValueForOption(_duplicateVersionFailsOption);
            });

        SubscribeEvent(
            ConfigurationEvents.CreatedConfiguration.Discriminator,
            configuration => _configuration = configuration);

        SubscribeEvent(NextVersionEvents.ConfiguredGlobalServices.Discriminator, services => {
            EventAggregator.PublishEvent(GitEvents.ConfiguringGlobalServices.Discriminator, services);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .UseConfigurationDefaults(_configuration)));

            EventAggregator.PublishEvent(GitEvents.ConfiguredGlobalServices.Discriminator, services);
        });

        SubscribeEvent(NextVersionEvents.ConfiguredCalculationServices.Discriminator, services => {
            EventAggregator.PublishEvent(GitEvents.ConfiguringCalculationServices.Discriminator, services);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(git => git
                    .UseLatestCommitVersion()
                    .UseActiveBranchCaseDefaults()
                    .UseCommitMessagesProvider()));

            if (!_options.ShouldOverrideVersioningMode) {
                services.ConfigureVernuntii(vernuntii => vernuntii
                    .ConfigureGit(git => git
                        .UseActiveBranchCaseVersioningMode()));
            }

            if (_overridePostPreRelease != null) {
                services.ConfigureVernuntii(features => features
                    .ConfigureGit(features => features
                        .ConfigurePreRelease(configurer => configurer
                            .SetPostPreRelease(_overridePostPreRelease))));
            }

            EventAggregator.PublishEvent(GitEvents.ConfiguredCalculationServices.Discriminator, services);
        });

        IRepository repository = null!;

        SubscribeEvent(
                NextVersionEvents.CreatedCalculationServiceProvider.Discriminator,
                sp => repository = sp.GetRequiredService<IRepository>());

        SubscribeEvent(
            NextVersionEvents.CalculatedNextVersion.Discriminator, version => {
                if (_duplicateVersionFails && repository.HasCommitVersion(version)) {
                    _nextVersionPlugin.ExitCodeOnSuccess = (int)ExitCode.VersionDuplicate;
                }
            });
    }
}
