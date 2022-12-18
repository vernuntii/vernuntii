using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using Microsoft.Extensions.DependencyInjection;
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
    /// <summary>
    /// <inheritdoc/>
    /// If it has been set manually then this will be
    /// used instead of the directory of the config path
    /// to resolve the git directory via the git directory resolver.
    /// Retrievable after <see cref="ConfigurationEvents.ConfiguredConfigurationBuilder"/>.
    /// </summary>
    [AllowNull]
    public string WorkingTreeDirectory {
        get => _resolvedWorkingTreeDirectory ?? _workingTreeDirectory ?? throw new InvalidOperationException("Working tree directory is not set");

        set {
            EnsureNotHavingConfiguredConfigurationBuilder();
            _workingTreeDirectory = value;
        }
    }

    /// <summary>
    /// The git command.
    /// Available after <see cref="ConfigurationEvents.ConfiguredConfigurationBuilder"/>.
    /// </summary>
    public IGitCommand GitCommand => _gitCommand ?? throw new InvalidOperationException($"The event \"{nameof(ConfigurationEvents.ConfiguredConfigurationBuilder)}\" must be called");

    private readonly SharedOptionsPlugin _sharedOptions = null!;
    private IGitCommand? _gitCommand;
    private readonly INextVersionPlugin _nextVersionPlugin = null!;
    private ISignalCounter _configuredConfigurationBuilderEventSignal = null!;

    private readonly Option<string?> _overridePostPreReleaseOption = new(new[] { "--override-post-pre-release" });

    private readonly Option<bool> _duplicateVersionFailsOption = new(new string[] { "--duplicate-version-fails" }) {
        Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
    };

    private string? _workingTreeDirectory;
    private string? _resolvedWorkingTreeDirectory;
    private readonly ILogger _logger = null!;
    private GitCommandFactoryRequest? _gitCommandFactoryRequest = null!;
    //private IRepository? _alternativeRepository;
    //private IGitCommand? _alternativeGitCommand;

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

    [MemberNotNull(nameof(_gitCommandFactoryRequest))]
    private void EnsureRequestedGitDirectoryFactory()
    {

        if (_gitCommandFactoryRequest != null) {
            return;
        }

        _gitCommandFactoryRequest = new GitCommandFactoryRequest();
        Events.FireEvent(GitEvents.RequestGitCommandFactory, _gitCommandFactoryRequest);
    }

    [MemberNotNull(nameof(_resolvedWorkingTreeDirectory))]
    private void EnsureResolvedWorkingTreeDirectory()
    {
        if (_resolvedWorkingTreeDirectory != null) {
            return;
        }

        string directoryToResolve;
        var alternativeWorkingTreeDirectory = _workingTreeDirectory;

        if (alternativeWorkingTreeDirectory == null) {
            /* Due to no alternative directory, we use the configuration path as starting point */
            var configPath = _sharedOptions.ConfigPath;

            if (File.Exists(configPath)) {
                directoryToResolve = Path.GetDirectoryName(configPath) ?? throw new InvalidOperationException("Due to no alternative directory, we use the directory of the configuration path as starting point, but the directory is null");
            } else {
                directoryToResolve = configPath;
            }
        } else {
            directoryToResolve = alternativeWorkingTreeDirectory;
        }

        EnsureRequestedGitDirectoryFactory();
        var gitDirectoryResolver = _gitCommandFactoryRequest.GitDirectoryResolver ?? GitDirectoryResolver.Default;
        _resolvedWorkingTreeDirectory = gitDirectoryResolver.ResolveWorkingTreeDirectory(directoryToResolve);
    }

    [MemberNotNull(nameof(_gitCommand))]
    private void EnsureCreatedGitCommand()
    {
        if (_gitCommand != null) {
            return;
        }

        EnsureRequestedGitDirectoryFactory();
        EnsureResolvedWorkingTreeDirectory();

        _gitCommand = (_gitCommandFactoryRequest.GitCommandFactory ?? GitCommandFactory.Default).CreateCommand(_resolvedWorkingTreeDirectory);

        if (_resolvedWorkingTreeDirectory != _gitCommand.WorkingTreeDirectory) {
            _resolvedWorkingTreeDirectory = _gitCommand.WorkingTreeDirectory;
        }

        _logger.LogInformation("Use repository directory: {_workingTreeDirectory}", _resolvedWorkingTreeDirectory);
        Events.FireEvent(GitEvents.CreatedGitCommand, _gitCommand);
    }

    /// <inheritdoc/>
    protected override void OnExecution()
    {
        _configuredConfigurationBuilderEventSignal = Events.OnNextEvent(
            ConfigurationEvents.ConfiguredConfigurationBuilder,
            EnsureCreatedGitCommand);

        var eachCommandLineParseResult = Events.GetEvent(CommandLineEvents.ParsedCommandLineArgs);

        Events.GetEvent(NextVersionEvents.ConfigureGlobalServices)
            .WithLatestFrom(eachCommandLineParseResult.Select(parseResult => parseResult.GetValueForOption(_overridePostPreReleaseOption)))
            .WithLatestFrom(Events.GetEvent(ConfigurationEvents.CreatedConfiguration))
            .Subscribe(result => {
                // TODO: move above configuration stuff here
                var ((services, overridePostPreRelease), configuration) = result;
                Events.FireEvent(GitEvents.ConfiguringGlobalServices, services);

                EnsureCreatedGitCommand();
                services.AddSingleton(_gitCommand);

                services
                    .ScopeToVernuntii(features => features
                    .ScopeToGit(git => git
                        .AddRepository()
                        .UseConfigurationDefaults(configuration)
                        .UseLatestCommitVersion()
                        .UseActiveBranchCaseDefaults()
                        .UseCommitMessagesProvider()));

                if (!_sharedOptions.ShouldOverrideVersioningMode) {
                    services
                        .ScopeToVernuntii(vernuntii => vernuntii
                        .ScopeToGit(git => git
                            .UseActiveBranchCaseVersioningMode()));
                }

                if (overridePostPreRelease != null) {
                    services
                        .ScopeToVernuntii(features => features
                        .ScopeToGit(features => features
                            .Configure(configurer => configurer
                                .SetPostPreRelease(overridePostPreRelease))));
                }

                Events.FireEvent(GitEvents.ConfiguredGlobalServices, services);
            });

        // On next version calculation we want to set bad exit code if equivalent commit version already exists
        Events.GetEvent(NextVersionEvents.CalculatedNextVersion)
            .WithLatestFrom(eachCommandLineParseResult.Select(parseResult => parseResult.GetValueForOption(_duplicateVersionFailsOption)))
            .WithLatestFrom(Events.GetEvent(NextVersionEvents.CreatedScopedServiceProvider).Select(sp => sp.GetRequiredService<IRepository>()))
            .Subscribe(result => {
                var ((versionCache, duplicateVersionFails), repository) = result;

                if (duplicateVersionFails && repository.HasCommitVersion(versionCache.Version)) {
                    _nextVersionPlugin.ExitCodeOnSuccess = (int)ExitCode.VersionDuplicate;
                }
            });
    }
}
