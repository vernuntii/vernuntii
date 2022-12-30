using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Meta;
using Vernuntii.PluginSystem.Reactive;

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
    private bool _isConfiguredConfigurationBuilder;

    private readonly Option<string?> _overridePostPreReleaseOption = new(new[] { "--override-post-pre-release" });

    private readonly Option<bool> _duplicateVersionFailsOption = new(new string[] { "--duplicate-version-fails" }) {
        Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
    };

    private string? _workingTreeDirectory;
    private string? _resolvedWorkingTreeDirectory;
    private readonly ILogger _logger = null!;
    private GitCommandFactoryRequest? _gitCommandFactoryRequest;

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
        if (_isConfiguredConfigurationBuilder) {
            throw new InvalidOperationException($"The event \"{nameof(ConfigurationEvents.ConfiguredConfigurationBuilder)}\" was already called");
        }
    }

    [MemberNotNull(nameof(_gitCommandFactoryRequest))]
    private async ValueTask EnsureRequestedGitDirectoryFactory()
    {

        if (_gitCommandFactoryRequest != null) {
            return;
        }

        _gitCommandFactoryRequest = new GitCommandFactoryRequest();
        await Events.FulfillAsync(GitEvents.RequestGitCommandFactory, _gitCommandFactoryRequest).ConfigureAwait(false);
    }

    [MemberNotNull(nameof(_resolvedWorkingTreeDirectory))]
    private async ValueTask EnsureResolvedWorkingTreeDirectory(string? configFile)
    {
        if (_resolvedWorkingTreeDirectory != null) {
            return;
        }

        string directoryToResolve;
        var alternativeWorkingTreeDirectory = _workingTreeDirectory;

        if (alternativeWorkingTreeDirectory == null) {
            if (File.Exists(configFile)) {
                // Due to no alternative directory, we use the configuration path as starting point
                directoryToResolve = Path.GetDirectoryName(configFile) ?? throw new InvalidOperationException("Due to no alternative directory, we use the directory of the configuration path as starting point, but the directory is null");
            } else {
                directoryToResolve = configFile ?? Directory.GetCurrentDirectory();
            }
        } else {
            directoryToResolve = alternativeWorkingTreeDirectory;
        }

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        await EnsureRequestedGitDirectoryFactory().ConfigureAwait(false);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

        var gitDirectoryResolver = _gitCommandFactoryRequest.GitDirectoryResolver ?? GitDirectoryResolver.Default;
        _resolvedWorkingTreeDirectory = gitDirectoryResolver.ResolveWorkingTreeDirectory(directoryToResolve);
    }

    [MemberNotNull(nameof(_gitCommand))]
    private async ValueTask EnsureCreatedGitCommand(string? configFile)
    {
        if (_gitCommand != null) {
            return;
        }

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        await EnsureRequestedGitDirectoryFactory().ConfigureAwait(false);
        await EnsureResolvedWorkingTreeDirectory(configFile).ConfigureAwait(false);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

        _gitCommand = (_gitCommandFactoryRequest.GitCommandFactory ?? GitCommandFactory.Default).CreateCommand(_resolvedWorkingTreeDirectory);

        if (_resolvedWorkingTreeDirectory != _gitCommand.WorkingTreeDirectory) {
            _resolvedWorkingTreeDirectory = _gitCommand.WorkingTreeDirectory;
        }

        _logger.LogInformation("Use repository directory: {_workingTreeDirectory}", _resolvedWorkingTreeDirectory);
        await Events.FulfillAsync(GitEvents.CreatedGitCommand, _gitCommand).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void OnExecution()
    {
        Events
            .Earliest(ConfigurationEvents.ConfiguredConfigurationBuilder)
            .Subscribe(async result => {
                _isConfiguredConfigurationBuilder = true;
                await EnsureCreatedGitCommand(result.ConfigPath).ConfigureAwait(false);
            })
            .DisposeWhenDisposing(this);

        var nextCommandLineParseResult = Events.Earliest(CommandLineEvents.ParsedCommandLineArguments);

        Events.Earliest(NextVersionEvents.ConfigureServices)
            .Zip(ConfigurationEvents.ConfiguredConfigurationBuilder)
            .Zip(nextCommandLineParseResult.Transform(parseResult => parseResult.GetValueForOption(_overridePostPreReleaseOption)))
            .Zip(ConfigurationEvents.CreatedConfiguration)
            .Subscribe(async result => {
                var (((services, configurationBuilderResult), overridePostPreRelease), configuration) = result;
                await Events.FulfillAsync(GitEvents.ConfigureServices, services).ConfigureAwait(false);

                await EnsureCreatedGitCommand(configurationBuilderResult.ConfigPath).ConfigureAwait(false);
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

                await Events.FulfillAsync(GitEvents.ConfiguredServices, services).ConfigureAwait(false);
            });

        //nextCommandLineParseResult.Transform(parseResult => parseResult.GetValueForOption(_duplicateVersionFailsOption)).Replay(1).RefCount().Subscribe(out var nextDuplicateVersionFails).DisposeWhenDisposing(this);
        //Events.Earliest(NextVersionEvents.CreatedScopedServiceProvider).Transform(sp => sp.GetRequiredService<IRepository>()).Replay(1).RefCount().Subscribe(out var nextCreatedScopedServiceProvider).DisposeWhenDisposing(this);

        // On next version calculation we want to set bad exit code if equivalent commit version already exists
        Events.Every(NextVersionEvents.CalculatedNextVersion)
            //.WithAwaitedFrom(nextDuplicateVersionFails)
            //.WithAwaitedFrom(nextCreatedScopedServiceProvider)
            .Zip(nextCommandLineParseResult.Transform(parseResult => parseResult.GetValueForOption(_duplicateVersionFailsOption)))
            .Zip(Events.Earliest(NextVersionEvents.CreatedScopedServiceProvider).Transform(sp => sp.GetRequiredService<IRepository>()))
            .Subscribe(result => {
                var ((versionCache, duplicateVersionFails), repository) = result;

                if (duplicateVersionFails && repository.HasCommitVersion(versionCache.Version)) {
                    _nextVersionPlugin.ExitCodeOnSuccess = (int)ExitCode.VersionDuplicate;
                }
            })
            .DisposeWhenDisposing(this);
    }
}
