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
        private VersionCacheManager _versionCacheManager = null!;
        private IVersionCache? _versionCache;
        private bool _isChecked;

        public VersionCachePlugin(
            ILogger<VersionCachePlugin> logger,
            ILogger<VersionCacheManager> gitCommandLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _versionCacheManagerLogger = gitCommandLogger;
        }

        private void ThrowIfNotChecked()
        {
            if (!_isChecked) {
                throw new InvalidOperationException("The version cache check has not been processed (If you depend on it you need to arrange your stuff after that check)");
            }
        }

        private async Task CheckVersionCache(string? configFile, IGitCommand gitCommand, VersionCacheOptions versionCacheOptions)
        {
            var watch = new Stopwatch();
            watch.Start();

            var gitDirectory = gitCommand.GetGitDirectory();
            var versionCacheDirectory = new VersionCacheDirectory(new VersionCacheDirectoryOptions(gitDirectory));

            var versionCacheManagerOptions = new VersionCacheManagerContext();
            await Events.FulfillAsync(VersionCacheEvents.CreateVersionCacheManager, versionCacheManagerOptions);

            var messagePackVersionCacheFileFactory = MessagePackVersionCacheFileFactory.Of(
                versionCacheManagerOptions.Serializers.Values.Select(x => x.Formatter),
                versionCacheManagerOptions.Serializers.Values.Select(x => x.Deformatter));

            _versionCacheManager = new VersionCacheManager(
                versionCacheDirectory,
                new VersionCacheEvaluator(),
                versionCacheOptions,
                messagePackVersionCacheFileFactory,
                _versionCacheManagerLogger);

            var versionHashFile = new VersionHashFile(new VersionHashFileOptions(gitDirectory, configFile), _versionCacheManager, versionCacheDirectory);
            var isCacheUpToDate = !versionHashFile.IsVersionRecacheRequired(out var versionCache);

            if (isCacheUpToDate) {
                _versionCache = versionCache;
            }

            _isChecked = true;
            _logger.LogInformation("Checked version cache in {ElapsedTime} (Cache id = {CacheId}, Up-to-date = {UpToDate})", watch.Elapsed.ToSecondsString(), _versionCacheManager.CacheId, isCacheUpToDate);
            await Events.FulfillAsync(VersionCacheEvents.CheckedVersionCache);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.Every(ConfigurationEvents.ConfiguredConfigurationBuilder)
                .Zip(GitEvents.CreatedGitCommand)
                .Zip(VersionCacheOptionsEvents.ParsedVersionCacheOptions)
                .Zip(VersionCacheEvents.CheckVersionCache)
                .Subscribe(result => {
                    var (((configuredConfigurationBuilderResult, gitCommand), versionCacheOptions), _) = result;
                    return CheckVersionCache(configuredConfigurationBuilderResult.ConfigPath, gitCommand, versionCacheOptions);
                });

            Events.Every(ServicesEvents.ConfigureServices)
                .Zip(VersionCacheEvents.CheckedVersionCache)
                .Subscribe(result => {
                    var (services, _) = result;
                    services.AddSingleton<IVersionCacheManager>(_versionCacheManager);
                });
        }
    }
}
