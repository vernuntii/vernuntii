using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Diagnostics;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.Plugins.VersionPersistence;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.VersionPersistence;
using Vernuntii.VersionPersistence.IO;
using static Vernuntii.PluginSystem.Reactive.EventChainFactoryExtensions;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the up-to date mechanism to check whether you need to re-calculate the next version.
    /// </summary>
    [ImportPlugin<VersionCacheOptionsPlugin>(TryRegister = true)]
    internal class VersionCachePlugin : Plugin, IVersionCachePlugin
    {
        /// <inheritdoc/>
        public IVersionCache? VersionCache {
            get {
                ThrowIfNotChecked();
                return _versionCache;
            }
        }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(VersionCache))]
        public bool IsCacheUpToDate => _versionCache != null;

        private readonly ILogger<VersionCachePlugin> _logger;
        private readonly ILogger<VersionCacheManager> _versionCacheManagerLogger;
        private VersionCacheManager? _versionCacheManager;
        private string? _gitDirectory;
        private VersionCacheDirectory? _versionCacheDirectory;
        private VersionHashFile? _versionHashFile;
        private IVersionCache? _versionCache;
        private bool _isChecked;

        public VersionCachePlugin(
            ILogger<VersionCachePlugin> logger,
            ILogger<VersionCacheManager> cersionCacheManagerLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _versionCacheManagerLogger = cersionCacheManagerLogger;
        }

        private void ThrowIfNotChecked()
        {
            if (!_isChecked) {
                throw new InvalidOperationException("The version cache check has not been processed (If you depend on it you need to arrange your stuff after that check)");
            }
        }

        [MemberNotNull(nameof(_gitDirectory), nameof(_versionCacheDirectory), nameof(_versionCacheManager), nameof(_versionHashFile))]
        private async Task CreateVersionCacheCheckDependenciesOnce(string? configFile, IGitCommand gitCommand, VersionCacheOptions versionCacheOptions)
        {
            if (_gitDirectory is not null && _versionCacheDirectory is not null && _versionCacheManager is not null && _versionHashFile is not null) {
                return;
            }

            _gitDirectory = gitCommand.GetGitDirectory();
            _versionCacheDirectory = new VersionCacheDirectory(new VersionCacheDirectoryOptions(_gitDirectory));

            var versionCacheManagerOptions = new VersionCacheManagerContext();

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
            await Events.EmitAsync(VersionCacheEvents.OnCreateVersionCacheManager, versionCacheManagerOptions);
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

            var messagePackVersionCacheFileFactory = MessagePackVersionCacheFileFactory.Of(
                versionCacheManagerOptions.Serializers.Values.Select(x => x.Formatter),
                versionCacheManagerOptions.Serializers.Values.Select(x => x.Deformatter));

            _versionCacheManager = new VersionCacheManager(
                _versionCacheDirectory,
                new VersionCacheEvaluator(),
                versionCacheOptions,
                messagePackVersionCacheFileFactory,
                _versionCacheManagerLogger);

            _versionHashFile = new VersionHashFile(new VersionHashFileOptions(_gitDirectory, configFile), _versionCacheManager, _versionCacheDirectory);
        }

        private async Task CheckVersionCache(string? configFile, IGitCommand gitCommand, VersionCacheOptions versionCacheOptions)
        {
            var watch = new Stopwatch();
            watch.Start();

            await CreateVersionCacheCheckDependenciesOnce(configFile, gitCommand, versionCacheOptions);
            var hash = await _versionHashFile.IsHashUpToDateOtherwiseUpdate();
            string? cacheNotUpToDateReason;

            if (hash.IsUpTodate) {
                if (_versionCacheManager.IsCacheUpToDate(out var versionCache, out cacheNotUpToDateReason)) {
                    _versionCache = versionCache;
                }
            } else {
                cacheNotUpToDateReason = hash.Reason;
            }

            _isChecked = true;

            _logger.LogInformation(
                "Checked version cache in {ElapsedTime} (Cache id = {CacheId}, Up-to-date = {UpToDate}{NotUpToDateReason})",
                watch.Elapsed.ToSecondsString(),
                _versionCacheManager.CacheId,
                IsCacheUpToDate,
                cacheNotUpToDateReason != null ? ", Reason = " + cacheNotUpToDateReason : "");

            await Events.EmitAsync(VersionCacheEvents.OnCheckedVersionCache);
        }

        [MemberNotNull(nameof(_versionCacheManager))]
        private VersionCacheManager GetVersionCacheManager() =>
            _versionCacheManager ?? throw new InvalidOperationException("The version manager has not been created");

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.OnceEveryXReplayOneTimeXY(
                    LifecycleEvents.BeforeEveryRun,
                    Events.OneTime(ConfigurationEvents.OnConfiguredConfigurationBuilder)
                        .And(GitEvents.OnCreatedGitCommand, Every)
                        .And(VersionCacheOptionsEvents.OnParsedVersionCacheOptions))
                .And(VersionCacheEvents.CheckVersionCache)
                .Subscribe(result => {
                    var ((_, ((configuredConfigurationBuilderResult, gitCommand), versionCacheOptions)), _) = result;
                    return CheckVersionCache(configuredConfigurationBuilderResult.ConfigPath, gitCommand, versionCacheOptions);
                });

            Events.Every(ServicesEvents.OnConfigureServices)
                .And(VersionCacheEvents.OnCheckedVersionCache)
                .Subscribe(result => {
                    var (services, _) = result;
                    services.AddSingleton<IVersionCacheManager>(GetVersionCacheManager());
                });
        }
    }
}
