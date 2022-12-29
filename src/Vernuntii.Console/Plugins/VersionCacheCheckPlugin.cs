using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
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
                EnsureHavingVersionChecked();
                return _versionCache;
            }
        }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(VersionCache))]
        public bool IsCacheUpToDate {
            get {
                EnsureHavingVersionChecked();
                return VersionCache != null;
            }
        }

        //private readonly IConfigurationPlugin _configurationPlugin;
        private readonly VersionCacheOptionsPlugin _versionCacheOptionsPlugin;
        private readonly ILogger<VersionCacheCheckPlugin> _logger;
        private readonly ILogger<VersionCacheManager> _versionCacheManagerLogger;
        private VersionCacheManager _versionCacheManager = null!;
        private IVersionCache? _versionCache;
        private bool _isVersionChecked;

        public VersionCacheCheckPlugin(
            //IConfigurationPlugin configurationPlugin,
            VersionCacheOptionsPlugin versionCacheOptionsPlugin,
            ILogger<VersionCacheCheckPlugin> logger,
            ILogger<VersionCacheManager> gitCommandLogger)
        {
            //_configurationPlugin = configurationPlugin ?? throw new ArgumentNullException(nameof(configurationPlugin));
            _versionCacheOptionsPlugin = versionCacheOptionsPlugin ?? throw new ArgumentNullException(nameof(versionCacheOptionsPlugin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _versionCacheManagerLogger = gitCommandLogger;
        }

        private void EnsureHavingVersionChecked()
        {
            if (!_isVersionChecked) {
                throw new InvalidOperationException("The version cache check has not been processed (If you depend on it you need to arrange your stuff after that check)");
            }
        }

        private async ValueTask CreateVersionCacheManager(string? configFile, IGitCommand gitCommand)
        {
            var gitDirectory = gitCommand.GetGitDirectory();
            var versionCacheDirectory = new VersionCacheDirectory(new VersionCacheDirectoryOptions(gitDirectory));

            _versionCacheManager = new VersionCacheManager(
                versionCacheDirectory,
                new VersionCacheEvaluator(),
                _versionCacheOptionsPlugin.CacheOptions,
                _versionCacheManagerLogger);

            var versionHashFile = new VersionHashFile(new VersionHashFileOptions(gitDirectory, configFile), _versionCacheManager, versionCacheDirectory, _logger);

            if (!versionHashFile.IsRecacheRequired(out var versionCache)) {
                _versionCache = versionCache;
            }

            _isVersionChecked = true;
            await Events.FulfillAsync(VersionCacheCheckEvents.CheckedVersionCache).ConfigureAwait(true);
        }

        private void OnVersionCacheCheck()
        {

        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.Every(ConfigurationEvents.ConfiguredConfigurationBuilder)
                .Zip(GitEvents.CreatedGitCommand)
                .Subscribe(async result => {
                    var (configuredConfigurationBuilderResult, gitCommand) = result;
                    await CreateVersionCacheManager(configuredConfigurationBuilderResult.ConfigPath, gitCommand).ConfigureAwait(true);
                })
                .DisposeWhenDisposing(this);

            Events.Every(ServicesEvents.ConfigureServices)
                .Zip(VersionCacheCheckEvents.CheckedVersionCache)
                .Subscribe(result => {
                    var (services, _) = result;
                    services.AddSingleton<IVersionCacheManager>(_versionCacheManager);
                })
                .DisposeWhenDisposing(this);

            Events.Earliest(VersionCacheCheckEvents.CheckVersionCache).Subscribe(OnVersionCacheCheck).DisposeWhenDisposing(this);
        }
    }
}
