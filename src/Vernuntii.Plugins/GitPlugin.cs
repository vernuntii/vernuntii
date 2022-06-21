using System.CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private const string DefaultGitPointerFile = "vernuntii.git";

    /// <summary>
    /// <inheritdoc/>
    /// If it has been set manually then this will be
    /// used instead of the directory of the config path
    /// to resolve the git directory via
    /// <see cref="GitDirectoryResolver"/>.
    /// </summary>
    public string GitDirectory {
        get => _gitDirectory ?? throw new InvalidOperationException("Git directory is not set");
        set => _gitDirectory = value;
    }

    /// <summary>
    /// If set before <see cref="ConfigurationEvents.ConfiguredConfigurationBuilder"/>
    /// it will be used for resolving the git directory. Otherwise an exception is
    /// thrown. The default behaviour is to look for a vernuntii.git-file pointing
    /// to another location, a .git-folder or a .git-file.
    /// </summary>
    public IGitDirectoryResolver? GitDirectoryResolver { get; set; }

    private SharedOptionsPlugin _sharedOptions = null!;
    private IConfiguration _configuration = null!;
    private INextVersionPlugin _nextVersionPlugin = null!;

    private Option<string?> _overridePostPreReleaseOption = new Option<string?>(new[] { "--override-post-pre-release" });

    private Option<bool> _duplicateVersionFailsOption = new Option<bool>(new string[] { "--duplicate-version-fails" }) {
        Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
    };

    private string? _overridePostPreRelease;
    private bool _duplicateVersionFails;
    private string? _gitDirectory;
    private ILogger _logger = null!;

    /// <inheritdoc/>
    protected override async ValueTask OnRegistrationAsync(RegistrationContext registrationContext) =>
        await registrationContext.PluginRegistry.TryRegisterAsync<SharedOptionsPlugin>();

    /// <inheritdoc/>
    protected override void OnCompletedRegistration()
    {
        Plugins.FirstLazy<ICommandLinePlugin>().Registered += plugin => {
            plugin.RootCommand.Add(_overridePostPreReleaseOption);
            plugin.RootCommand.Add(_duplicateVersionFailsOption);
        };

        _sharedOptions = Plugins.First<SharedOptionsPlugin>();
        _nextVersionPlugin = Plugins.First<INextVersionPlugin>();
        _logger = Plugins.First<ILoggingPlugin>().CreateLogger<GitPlugin>();
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
            ConfigurationEvents.ConfiguredConfigurationBuilder,
            () => {
                string directoryToResolve;
                var customGitDirectory = _gitDirectory;

                if (customGitDirectory == null) {
                    var configPath = _sharedOptions.ConfigPath;

                    if (File.Exists(configPath)) {
                        directoryToResolve = Path.GetDirectoryName(configPath) ?? throw new InvalidOperationException("Configuration directory was expected");
                    } else {
                        directoryToResolve = configPath;
                    }
                } else {
                    directoryToResolve = customGitDirectory;
                }

                var gitDirectoryResolver = GitDirectoryResolver ?? new AlternativeGitDirectoryResolver(
                    Path.Combine(directoryToResolve, DefaultGitPointerFile),
                    DefaultGitDirectoryResolver.Default);

                var resolvedGitDirectory = gitDirectoryResolver.ResolveGitDirectory(directoryToResolve);
                _logger.LogInformation("Use repository directory: {_gitDirectory}", resolvedGitDirectory);
                GitDirectory = resolvedGitDirectory;
                Events.Publish(GitEvents.ResolvedGitDirectory, resolvedGitDirectory);
            });

        Events.SubscribeOnce(
            ConfigurationEvents.CreatedConfiguration,
            configuration => _configuration = configuration);

        Events.SubscribeOnce(NextVersionEvents.ConfiguredGlobalServices, services => {
            Events.Publish(GitEvents.ConfiguringGlobalServices, services);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .AddRepository(options => {
                        options.GitDirectory = GitDirectory;
                        options.GitDirectoryResolver = InOutGitDirectoryProvider.Instance;
                    })
                    .UseConfigurationDefaults(_configuration)));

            Events.Publish(GitEvents.ConfiguredGlobalServices, services);
        });

        Events.Subscribe(NextVersionEvents.ConfiguredCalculationServices, services => {
            Events.Publish(GitEvents.ConfiguringCalculationServices, services);

            services.ConfigureVernuntii(features => features
                .ConfigureGit(git => git
                    .UseLatestCommitVersion()
                    .UseActiveBranchCaseDefaults()
                    .UseCommitMessagesProvider()));

            if (!_sharedOptions.ShouldOverrideVersioningMode) {
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

        Events.Subscribe(
                NextVersionEvents.CreatedCalculationServiceProvider,
                sp => repository = sp.GetRequiredService<IRepository>());

        Events.Subscribe(
            NextVersionEvents.CalculatedNextVersion, versionCache => {
                if (_duplicateVersionFails && repository.HasCommitVersion(versionCache.Version)) {
                    _nextVersionPlugin.ExitCodeOnSuccess = (int)ExitCode.VersionDuplicate;
                }
            });

        Events.Subscribe(GitEvents.UnsetRepositoryCache, () => repository?.UnsetCache());
    }
}
