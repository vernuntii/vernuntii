using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins;

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
        await Plugins.TryRegisterAsync<NextVersionOptionsPlugin>();

    /// <inheritdoc/>
    protected override void OnCompletedRegistration()
    {
        Plugins.First<ICommandLinePlugin>().Registered += plugin => {
            plugin.RootCommand.Add(_overridePostPreReleaseOption);
            plugin.RootCommand.Add(_duplicateVersionFailsOption);
        };

        _options = Plugins.First<NextVersionOptionsPlugin>().Value;
        _nextVersionPlugin = Plugins.First<INextVersionPlugin>().Value;
    }

    /// <inheritdoc/>
    protected override void OnEvents()
    {
        Events.SubscribeOnce(
            CommandLineEvents.ParsedCommandLineArgs,
            parseResult => {
                _overridePostPreRelease = parseResult.GetValueForOption(_overridePostPreReleaseOption);
                _duplicateVersionFails = parseResult.GetValueForOption(_duplicateVersionFailsOption);
            });

        Events.SubscribeOnce(
            ConfigurationEvents.CreatedConfiguration,
            configuration => _configuration = configuration);

        Events.SubscribeOnce(NextVersionEvents.ConfiguredGlobalServices, services => {
            Events.Publish(GitEvents.ConfiguringGlobalServices, services);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .UseConfigurationDefaults(_configuration)));

            Events.Publish(GitEvents.ConfiguredGlobalServices, services);
        });

        Events.SubscribeOnce(NextVersionEvents.ConfiguredCalculationServices, services => {
            Events.Publish(GitEvents.ConfiguringCalculationServices, services);

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

            Events.Publish(GitEvents.ConfiguredCalculationServices, services);
        });

        IRepository repository = null!;

        Events.SubscribeOnce(
                NextVersionEvents.CreatedCalculationServiceProvider,
                sp => repository = sp.GetRequiredService<IRepository>());

        Events.SubscribeOnce(
            NextVersionEvents.CalculatedNextVersion, version => {
                if (_duplicateVersionFails && repository.HasCommitVersion(version)) {
                    _nextVersionPlugin.ExitCodeOnSuccess = (int)ExitCode.VersionDuplicate;
                }
            });
    }
}
