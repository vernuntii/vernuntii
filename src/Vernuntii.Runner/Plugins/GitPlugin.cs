using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Git;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.Runner;
using Vernuntii.VersionPersistence;
using Vernuntii.VersionPersistence.Presentation;
using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.Plugins;

/// <summary>
/// The git plugin.
/// </summary>
[ImportPlugin<SharedOptionsPlugin>(TryRegister = true)]
[ImportPlugin<IVersionCachePlugin, VersionCachePlugin>(TryRegister = true)]
public class GitPlugin : Plugin, IGitPlugin
{
    /// <summary>
    /// The identifier is used when the <see cref="IVersionCacheFormatter"/> and <see cref="IVersionCacheDeformatter"/> are registered
    /// to <see cref="VersionCacheManagerContext.Serializers"/>.
    /// </summary>
    public static object VersionCacheManagerSerializerIdentifier = typeof(GitPlugin);

    /// <summary>
    /// <inheritdoc/>
    /// If it has been set manually then this will be
    /// used instead of the directory of the config path
    /// to resolve the git directory via the git directory resolver.
    /// Retrievable after <see cref="ConfigurationEvents.OnConfiguredConfigurationBuilder"/>.
    /// </summary>
    [AllowNull]
    public string WorkingTreeDirectory {
        get => _resolvedWorkingTreeDirectory ?? _workingTreeDirectory ?? throw new InvalidOperationException("Working tree directory is not set");

        set {
            EnsureNotYetConfiguredConfigurationBuilder();
            _workingTreeDirectory = value;
        }
    }

    /// <summary>
    /// The git command.
    /// Available after <see cref="ConfigurationEvents.OnConfiguredConfigurationBuilder"/>.
    /// </summary>
    public IGitCommand GitCommand => _gitCommand ?? throw new InvalidOperationException($"The event \"{nameof(ConfigurationEvents.OnConfiguredConfigurationBuilder)}\" must be called");

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
    private GitCommandCreationCustomization? _gitCommandFactoryRequest;

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

    private void EnsureNotYetConfiguredConfigurationBuilder()
    {
        if (_isConfiguredConfigurationBuilder) {
            throw new InvalidOperationException($"The event \"{nameof(ConfigurationEvents.OnConfiguredConfigurationBuilder)}\" was already called");
        }
    }

    [MemberNotNull(nameof(_gitCommandFactoryRequest))]
    private async ValueTask RequestGitDirectoryFactoryOnce()
    {
        if (_gitCommandFactoryRequest != null) {
            return;
        }

        _gitCommandFactoryRequest = new GitCommandCreationCustomization();
        await Events.EmitAsync(GitEvents.OnCustomizeGitCommandCreation, _gitCommandFactoryRequest).ConfigureAwait(false);
    }

    [MemberNotNull(nameof(_resolvedWorkingTreeDirectory))]
    private async ValueTask ResolveWorkingTreeDirectoryOnce(string? configFile)
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
        await RequestGitDirectoryFactoryOnce().ConfigureAwait(false);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

        var gitDirectoryResolver = _gitCommandFactoryRequest.GitDirectoryResolver ?? GitDirectoryResolver.Default;
        _resolvedWorkingTreeDirectory = gitDirectoryResolver.ResolveWorkingTreeDirectory(directoryToResolve);
    }

    [MemberNotNull(nameof(_gitCommand))]
    private async ValueTask CreateGitCommandOnce(string? configFile)
    {
        if (_gitCommand != null) {
            return;
        }

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
        await RequestGitDirectoryFactoryOnce().ConfigureAwait(false);
        await ResolveWorkingTreeDirectoryOnce(configFile).ConfigureAwait(false);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

        _gitCommand = (_gitCommandFactoryRequest.GitCommandFactory ?? GitCommandFactory.Default).CreateCommand(_resolvedWorkingTreeDirectory);

        if (_resolvedWorkingTreeDirectory != _gitCommand.WorkingTreeDirectory) {
            _resolvedWorkingTreeDirectory = _gitCommand.WorkingTreeDirectory;
        }

        _logger.LogInformation("Use repository directory: {_workingTreeDirectory}", _resolvedWorkingTreeDirectory);
        await Events.EmitAsync(GitEvents.OnCreatedGitCommand, _gitCommand).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override void OnExecution()
    {
        Events.Earliest(GitEvents.CreateGitCommand)
            .Zip(ConfigurationEvents.OnConfiguredConfigurationBuilder)
            .Subscribe(async result => {
                var (_, configurationBuilder) = result;
                _isConfiguredConfigurationBuilder = true;
                await CreateGitCommandOnce(configurationBuilder.ConfigPath).ConfigureAwait(false);
            });

        Events.Earliest(VersionCacheEvents.CreateVersionCacheManager)
            .Subscribe(context => context.ImportGitRequirements());

        Events.Earliest(NextVersionEvents.OnConfigureVersionPresentation)
            .Subscribe(context => context.ImportGitRequirements());

        var nextCommandLineParseResult = Events.Earliest(CommandLineEvents.ParsedCommandLineArguments);

        Events.Earliest(ServicesEvents.OnConfigureServices)
            .Subscribe(() => Events.EmitAsync(ConfigurationEvents.CreateConfiguration));

        Events.Earliest(NextVersionEvents.OnConfigureServices)
            .Zip(nextCommandLineParseResult.Transform(parseResult => parseResult.GetValueForOption(_overridePostPreReleaseOption)))
            .Zip(ConfigurationEvents.OnConfiguredConfigurationBuilder)
            .Zip(ConfigurationEvents.OnCreatedConfiguration)
            .Subscribe(async result => {
                var (((services, overridePostPreRelease), configurationBuilderResult), configuration) = result;

                await Events.EmitAsync(GitEvents.OnConfigureServices, services).ConfigureAwait(false);
                await CreateGitCommandOnce(configurationBuilderResult.ConfigPath).ConfigureAwait(false);

                services
                    .AddSingleton(_gitCommand)
                    .AddScoped<IVersionCacheDataTuplesEnricher, VersionCacheRepositoryDataEnricher>()
                    .TakeViewOfVernuntii()
                    .TakeViewOfGit()
                    .AddRepository()
                    .UseConfigurationDefaults(configuration)
                    .UseLatestCommitVersion()
                    .UseActiveBranchCaseDefaults()
                    .UseCommitMessagesProvider();

                if (!_sharedOptions.ShouldOverrideVersioningMode) {
                    services
                        .TakeViewOfVernuntii()
                        .TakeViewOfGit()
                        .UseActiveBranchCaseVersioningMode();
                }

                if (overridePostPreRelease != null) {
                    services
                        .TakeViewOfVernuntii()
                        .TakeViewOfGit()
                        .Configure(configurer => configurer
                            .SetPostPreRelease(overridePostPreRelease));
                }

                await Events.EmitAsync(GitEvents.OnConfiguredServices, services).ConfigureAwait(false);
            });

        // On next version calculation we want to set bad exit code if equivalent commit version already exists
        Events.Every(NextVersionEvents.OnCalculatedNextVersion)
            .Zip(NextVersionEvents.OnInvokedNextVersionCommand)
            .Zip(nextCommandLineParseResult.Transform(parseResult => parseResult.GetValueForOption(_duplicateVersionFailsOption)))
            .Zip(Events.Earliest(NextVersionEvents.OnCreatedScopedServiceProvider).Transform(sp => sp.GetRequiredService<IRepository>()))
            .Subscribe(result => {
                var (((nextVersionResult, commandResult), duplicateVersionFails), repository) = result;

                if (duplicateVersionFails && repository.HasCommitVersion(nextVersionResult.VersionCache.Version)) {
                    commandResult.ExitCode = (int)ExitCode.VersionDuplicate;
                }
            });
    }
}
