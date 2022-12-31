using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Diagnostics;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.Plugins.VersionCaching;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Meta;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.VersionCaching;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the up-to date mechanism to check whether you need to re-calculate the next version.
    /// </summary>
    [ImportPlugin<VersionCacheOptionsPlugin>(TryRegister = true)]
    internal class VersionCacheCheckPlugin : Plugin, IVersionCacheCheckPlugin
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
        public bool IsCacheUpToDate {
            get {
                ThrowIfNotChecked();
                return VersionCache != null;
            }
        }

        private readonly ILogger<VersionCacheCheckPlugin> _logger;
        private readonly ILogger<VersionCacheManager> _versionCacheManagerLogger;
        private VersionCacheManager _versionCacheManager = null!;
        private IVersionCache? _versionCache;
        private bool _isChecked;

        public VersionCacheCheckPlugin(
            ILogger<VersionCacheCheckPlugin> logger,
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

        private Task CheckVersionCache(string? configFile, IGitCommand gitCommand, VersionCacheOptions versionCacheOptions)
        {
            var watch = new Stopwatch();
            watch.Start();

            var gitDirectory = gitCommand.GetGitDirectory();
            var versionCacheDirectory = new VersionCacheDirectory(new VersionCacheDirectoryOptions(gitDirectory));

            _versionCacheManager = new VersionCacheManager(
                versionCacheDirectory,
                new VersionCacheEvaluator(),
                versionCacheOptions,
                _versionCacheManagerLogger);

            var versionHashFile = new VersionHashFile(new VersionHashFileOptions(gitDirectory, configFile), _versionCacheManager, versionCacheDirectory);
            var isCacheUpToDate = !versionHashFile.IsVersionRecacheRequired(out var versionCache);

            if (isCacheUpToDate) {
                _versionCache = versionCache;
            }

            _isChecked = true;
            _logger.LogInformation("Checked version cache in {ElapsedTime} (Cache id = {CacheId}, Up-to-date = {UpToDate})", watch.Elapsed.ToSecondsString(), _versionCacheManager.CacheId, isCacheUpToDate);
            return Events.FulfillAsync(VersionCacheCheckEvents.CheckedVersionCache);
        }

        private void OnVersionCacheCheck()
        {
            // TODO: Implement daemon cleanup
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.Every(ConfigurationEvents.ConfiguredConfigurationBuilder)
                .Zip(GitEvents.CreatedGitCommand)
                .Zip(VersionCacheOptionsEvents.ParsedVersionCacheOptions)
                .Subscribe(result => {
                    var ((configuredConfigurationBuilderResult, gitCommand), versionCacheOptions) = result;
                    return CheckVersionCache(configuredConfigurationBuilderResult.ConfigPath, gitCommand, versionCacheOptions);
                });

            Events.Every(ServicesEvents.ConfigureServices)
                .Zip(VersionCacheCheckEvents.CheckedVersionCache)
                .Subscribe(result => {
                    var (services, _) = result;
                    services.AddSingleton<IVersionCacheManager>(_versionCacheManager);
                });

            Events.Earliest(VersionCacheCheckEvents.CheckVersionCache).Subscribe(OnVersionCacheCheck);
        }
    }
}
