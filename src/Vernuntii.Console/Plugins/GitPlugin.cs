using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.Plugins;

/// <summary>
/// The git plugin.
/// </summary>
[ImportPlugin<SharedOptionsPlugin>(TryRegister = true)]
public class GitPlugin : Plugin, IGitPlugin
{
    private const string DefaultGitPointerFile = "vernuntii.git";

    /// <summary>
    /// <inheritdoc/>
    /// If it has been set manually then this will be
    /// used instead of the directory of the config path
    /// to resolve the git directory via
    /// <see cref="GitDirectoryResolver"/>.
    /// Available after <see cref="GitEvents.ResolvedGitWorkingTreeDirectory"/>.
    /// </summary>
    [AllowNull]
    public string WorkingTreeDirectory {
        get => _workingTreeDirectory ?? throw new InvalidOperationException("Working tree directory is not set");

        set {
            EnsureNotHavingConfiguredConfigurationBuilder();
            EnsureUnsetAlternativeRepository();
            _workingTreeDirectory = value;
        }
    }

    /// <summary>
    /// If set before <see cref="ConfigurationEvents.ConfiguredConfigurationBuilder"/>
    /// it will be used for resolving the git directory. If set after that event an exception
    /// is thrown. The default behaviour is to look for a vernuntii.git-file pointing to
    /// another location, a .git-folder or a .git-file.
    /// </summary>
    public IGitDirectoryResolver? GitDirectoryResolver {
        get => _gitDirectoryResolver;

        set {
            EnsureNotHavingConfiguredConfigurationBuilder();
            EnsureUnsetAlternativeRepository();
            _gitDirectoryResolver = value;
        }
    }

    /// <summary>
    /// The git command.
    /// Available after <see cref="GitEvents.ResolvedGitWorkingTreeDirectory"/>.
    /// </summary>
    public IGitCommand GitCommand => _gitCommand ?? throw new InvalidOperationException($"The event \"{nameof(GitEvents.ResolvedGitWorkingTreeDirectory)}\" was not yet called");

    private readonly SharedOptionsPlugin _sharedOptions = null!;
    private IGitCommand? _gitCommand;
    private readonly INextVersionPlugin _nextVersionPlugin = null!;
    private ISignalCounter _configuredConfigurationBuilderEventSignal = null!;

    private readonly Option<string?> _overridePostPreReleaseOption = new(new[] { "--override-post-pre-release" });

    private readonly Option<bool> _duplicateVersionFailsOption = new(new string[] { "--duplicate-version-fails" }) {
        Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
    };

    private string? _workingTreeDirectory;
    private readonly ILogger _logger = null!;
    private IGitDirectoryResolver? _gitDirectoryResolver;
    private IRepository? _alternativeRepository;
    private IGitCommand? _alternativeGitCommand;

    public GitPlugin(
        ICommandLinePlugin commandlinePlugin,
        SharedOptionsPlugin sharedOptions,
        INextVersionPlugin nextVersionPlugin,
        ILogger<GitPlugin> logger)
    {
        if (commandlinePlugin is null) {
            throw new ArgumentNullException(nameof(commandlinePlugin));
        }

        commandlinePlugin.RootCommand.Add(_overridePostPreReleaseOption);
        commandlinePlugin.RootCommand.Add(_duplicateVersionFailsOption);

        _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        _nextVersionPlugin = nextVersionPlugin ?? throw new ArgumentNullException(nameof(nextVersionPlugin));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private void EnsureNotHavingConfiguredConfigurationBuilder()
    {
        if (_configuredConfigurationBuilderEventSignal.IsOnceSignaled()) {
            throw new InvalidOperationException($"The event \"{nameof(ConfigurationEvents.ConfiguredConfigurationBuilder)}\" was already called");
        }
    }

    [MemberNotNullWhen(true, nameof(_alternativeRepository), nameof(_alternativeGitCommand))]
    private bool HavingAlternativeRepository() =>
        _alternativeRepository is not null && _alternativeGitCommand is not null;

    private void EnsureUnsetAlternativeRepository()
    {
        if (HavingAlternativeRepository()) {
            throw new InvalidOperationException("An alternative repository has been already set");
        }
    }

    private void EnsureUnsetWorkingTreeDirectory()
    {
        if (_workingTreeDirectory is not null) {
            throw new InvalidOperationException("The working tree directory is already set");
        }
    }

    private void EnsureUnsetGitDirectoryResolver()
    {
        if (_gitDirectoryResolver is not null) {
            throw new InvalidOperationException("The git directory resolver is already set");
        }
    }

    /// <inheritdoc/>
    public void SetAlternativeRepository(IRepository repository, IGitCommand gitCommand)
    {
        EnsureNotHavingConfiguredConfigurationBuilder();
        EnsureUnsetGitDirectoryResolver();
        EnsureUnsetWorkingTreeDirectory();

        _alternativeRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        _alternativeGitCommand = gitCommand ?? throw new ArgumentNullException(nameof(gitCommand));

        if (repository.GetGitDirectory() != gitCommand.GetGitDirectory()) {
            throw new ArgumentException("The git command points to a different .git-directory than the repository");
        }
    }

    /// <inheritdoc/>
    public void UnsetAlternativeRepository()
    {
        EnsureNotHavingConfiguredConfigurationBuilder();
        _alternativeRepository = null;
        _alternativeGitCommand = null;
    }

    private string ResolveWorkingTreeDirectory()
    {
        string directoryToResolve;
        var alternativeWorkingTreeDirectory = _workingTreeDirectory;

        if (alternativeWorkingTreeDirectory == null) {
            var configPath = _sharedOptions.ConfigPath;

            if (File.Exists(configPath)) {
                directoryToResolve = Path.GetDirectoryName(configPath) ?? throw new InvalidOperationException("Configuration directory was expected");
            } else {
                directoryToResolve = configPath;
            }
        } else {
            directoryToResolve = alternativeWorkingTreeDirectory;
        }

        var gitDirectoryResolver = GitDirectoryResolver ?? new AlternativeGitDirectoryResolver(
            Path.Combine(directoryToResolve, DefaultGitPointerFile),
            DefaultGitDirectoryResolver.Default);

        return gitDirectoryResolver.ResolveWorkingTreeDirectory(directoryToResolve);
    }

    private string GetWorkingTreeDirectory() =>
        HavingAlternativeRepository() switch {
            true => _alternativeGitCommand.WorkingTreeDirectory,
            false => ResolveWorkingTreeDirectory()
        };

    private void OnAfterConfiguredConfigurationBuilder()
    {
        var resolvedWorkingTreeDirectory = GetWorkingTreeDirectory();

        _logger.LogInformation("Use repository directory: {_workingTreeDirectory}", resolvedWorkingTreeDirectory);
        _workingTreeDirectory = resolvedWorkingTreeDirectory;
        Events.FireEvent(GitEvents.ResolvedGitWorkingTreeDirectory, resolvedWorkingTreeDirectory);

        if (HavingAlternativeRepository()) {
            _gitCommand = _alternativeGitCommand;
        } else {
            _gitCommand = new GitCommand(resolvedWorkingTreeDirectory);
        }

        Events.FireEvent(GitEvents.CreatedGitCommand, _gitCommand);
    }

    /// <inheritdoc/>
    protected override void OnExecution()
    {
        _configuredConfigurationBuilderEventSignal = Events.OnNextEvent(
            ConfigurationEvents.ConfiguredConfigurationBuilder,
            OnAfterConfiguredConfigurationBuilder);

        Events.OnNextEvent(GlobalServicesEvents.ConfigureServices, services => services
            .AddOptions<RepositoryOptions>()
                .Configure(options => options.GitCommandFactory = new GitCommandProvider(GitCommand)));

        Observable.CombineLatest(
                Events.GetEvent(ConfigurationEvents.CreatedConfiguration).Take(1),
                Events.GetEvent(NextVersionEvents.ConfigureGlobalServices),
                (configuration, services) => (configuration, services))
            .Subscribe(result => {
                var (configuration, services) = result;
                Events.FireEvent(GitEvents.ConfiguringGlobalServices, services);

                if (HavingAlternativeRepository()) {
                    services.TryAddSingleton(_alternativeRepository);
                }

                services.ScopeToVernuntii(features => features
                    .ScopeToGit(features => features
                        .AddRepository(options => {
                            options.GitWorkingTreeDirectory = WorkingTreeDirectory;
                            options.GitDirectoryResolver = GitDirectoryPassthrough.Instance;
                        })
                        .UseConfigurationDefaults(configuration)));

                Events.FireEvent(GitEvents.ConfiguredGlobalServices, services);
            });

        var firstCommandLineParseResult = Events.GetEvent(CommandLineEvents.ParsedCommandLineArgs);

        Observable.CombineLatest(
                firstCommandLineParseResult.Select(parseResult => parseResult.GetValueForOption(_overridePostPreReleaseOption)),
                Events.GetEvent(NextVersionEvents.ConfigureGlobalServices),
                (overridePostPreRelease, services) => (overridePostPreRelease, services))
            .Subscribe(result => {
                var (overridePostPreRelease, services) = result;
                Events.FireEvent(GitEvents.ConfiguringCalculationServices, services);

                services.ScopeToVernuntii(features => features
                    .ScopeToGit(git => git
                        .UseLatestCommitVersion()
                        .UseActiveBranchCaseDefaults()
                        .UseCommitMessagesProvider()));

                if (!_sharedOptions.ShouldOverrideVersioningMode) {
                    services.ScopeToVernuntii(vernuntii => vernuntii
                        .ScopeToGit(git => git
                            .UseActiveBranchCaseVersioningMode()));
                }

                if (overridePostPreRelease != null) {
                    services.ScopeToVernuntii(features => features
                        .ScopeToGit(features => features
                            .Configure(configurer => configurer
                                .SetPostPreRelease(overridePostPreRelease))));
                }

                Events.FireEvent(GitEvents.ConfiguredCalculationServices, services);
            });

        var eachRepository = Events.GetEvent(NextVersionEvents.CreatedScopedServiceProvider).Select(sp => sp.GetRequiredService<IRepository>());

        // Sets the exit code to indicate version duplicate in case of version duplicate
        Observable.CombineLatest(
                firstCommandLineParseResult.Select(parseResult => parseResult.GetValueForOption(_duplicateVersionFailsOption)),
                Observable.Zip(
                    eachRepository,
                    Events.GetEvent(NextVersionEvents.CalculatedNextVersion),
                    (repository, versionCache) => (repository, versionCache)),
                (duplicateVersionFails, zipResult) => (duplicateVersionFails, zipResult))
            .Subscribe(result => {
                var (duplicateVersionFails, (repository, versionCache)) = result;

                if (duplicateVersionFails && repository.HasCommitVersion(versionCache.Version)) {
                    _nextVersionPlugin.ExitCodeOnSuccess = (int)ExitCode.VersionDuplicate;
                }
            });

        // Reset repository cache *before* the next run starts
        Observable.Zip(
                eachRepository,
                Events.GetEvent(LifecycleEvents.BeforeNextRun),
                (repository, _) => (repository, _))
            .Subscribe(result => {
                var (repository, _) = result;
                repository.UnsetCache();
            });
    }
}
