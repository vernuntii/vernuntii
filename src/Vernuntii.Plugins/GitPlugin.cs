using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
using Vernuntii.Git.Command;
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
    /// Available after <see cref="GitEvents.ResolvedGitWorkingTreeDirectory"/>.
    /// </summary>
    [AllowNull]
    public string WorkingTreeDirectory {
        get => _workingTreeDirectory ?? throw new InvalidOperationException("Working tree directory is not set");

        set {
            EnsureBeingBeforeConfiguredConfigurationBuilderEvent();
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
            EnsureBeingBeforeConfiguredConfigurationBuilderEvent();
            EnsureUnsetAlternativeRepository();
            _gitDirectoryResolver = value;
        }
    }

    /// <summary>
    /// The git command.
    /// Available after <see cref="GitEvents.ResolvedGitWorkingTreeDirectory"/>.
    /// </summary>
    public IGitCommand GitCommand => _gitCommand ?? throw new InvalidOperationException($"The event \"{nameof(GitEvents.ResolvedGitWorkingTreeDirectory)}\" was not yet called");

    private SharedOptionsPlugin _sharedOptions = null!;
    private IConfiguration _configuration = null!;
    private IGitCommand? _gitCommand;
    private INextVersionPlugin _nextVersionPlugin = null!;
    private IOneSignal _configuredConfigurationBuilderEventSignal = null!;

    private Option<string?> _overridePostPreReleaseOption = new Option<string?>(new[] { "--override-post-pre-release" });

    private Option<bool> _duplicateVersionFailsOption = new Option<bool>(new string[] { "--duplicate-version-fails" }) {
        Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
    };

    private string? _overridePostPreRelease;
    private bool _duplicateVersionFails;
    private string? _workingTreeDirectory;
    private ILogger _logger = null!;
    private IGitDirectoryResolver? _gitDirectoryResolver;
    private IRepository? _alternativeRepository;
    private IGitCommand? _alternativeGitCommand;

    private void EnsureBeingBeforeConfiguredConfigurationBuilderEvent()
    {
        if (_configuredConfigurationBuilderEventSignal?.SignaledOnce ?? false) {
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
        EnsureBeingBeforeConfiguredConfigurationBuilderEvent();
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
        EnsureBeingBeforeConfiguredConfigurationBuilderEvent();
        _alternativeRepository = null;
        _alternativeGitCommand = null;
    }

    /// <inheritdoc/>
    protected override async ValueTask OnRegistrationAsync(RegistrationContext registrationContext) =>
        await registrationContext.PluginRegistry.TryRegisterAsync<SharedOptionsPlugin>();

    /// <inheritdoc/>
    protected override void OnAfterRegistration()
    {
        Plugins.FirstLazy<ICommandLinePlugin>().Registered += plugin => {
            plugin.RootCommand.Add(_overridePostPreReleaseOption);
            plugin.RootCommand.Add(_duplicateVersionFailsOption);
        };

        _sharedOptions = Plugins.First<SharedOptionsPlugin>();
        _nextVersionPlugin = Plugins.First<INextVersionPlugin>();
        _logger = Plugins.First<ILoggingPlugin>().CreateLogger<GitPlugin>();
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
        Events.Publish(GitEvents.ResolvedGitWorkingTreeDirectory, resolvedWorkingTreeDirectory);

        if (HavingAlternativeRepository()) {
            _gitCommand = _alternativeGitCommand;
        } else {
            _gitCommand = new GitCommand(resolvedWorkingTreeDirectory);
        }

        Events.Publish(GitEvents.CreatedGitCommand, _gitCommand);
    }

    private void OnConfigureGlobalServices(IServiceCollection services)
    {
        Events.Publish(GitEvents.ConfiguringGlobalServices, services);

        if (HavingAlternativeRepository()) {
            services.TryAddSingleton(_alternativeRepository);
        }

        services.ConfigureVernuntii(features => features
            .ConfigureGit(features => features
                .AddRepository(options => {
                    options.GitWorkingTreeDirectory = WorkingTreeDirectory;
                    options.GitDirectoryResolver = GitDirectoryPassthrough.Instance;
                })
                .UseConfigurationDefaults(_configuration)));

        Events.Publish(GitEvents.ConfiguredGlobalServices, services);
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

        _configuredConfigurationBuilderEventSignal = Events.SubscribeOnce(
            ConfigurationEvents.ConfiguredConfigurationBuilder,
            OnAfterConfiguredConfigurationBuilder);

        Events.SubscribeOnce(GlobalServicesEvents.ConfigureServices, services => services
            .AddOptions<RepositoryOptions>()
                .Configure(options => options.GitCommandFactory = new GitCommandProvider(GitCommand)));

        Events.SubscribeOnce(
            ConfigurationEvents.CreatedConfiguration,
            configuration => _configuration = configuration);

        Events.SubscribeOnce(NextVersionEvents.ConfiguredGlobalServices, OnConfigureGlobalServices);

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

        Events.Subscribe(LifecycleEvents.BeforeNextRun, () => repository?.UnsetCache());
    }
}
