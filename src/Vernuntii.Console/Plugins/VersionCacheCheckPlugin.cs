using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Meta;
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

        private readonly IConfigurationPlugin _configurationPlugin;
        private readonly VersionCacheOptionsPlugin _versionCacheOptionsPlugin;
        private readonly ILogger<VersionCacheCheckPlugin> _logger;
        private readonly ILogger<VersionCacheManager> _versionCacheManagerLogger;
        private IGitCommand _gitCommand = null!;
        private string? _configFile;
        private VersionCacheManager _versionCacheManager = null!;
        private VersionHashFile _versionHashFile = null!;
        private IVersionCache? _versionCache;
        private bool _isVersionChecked;

        public VersionCacheCheckPlugin(
            IConfigurationPlugin configurationPlugin,
            VersionCacheOptionsPlugin versionCacheOptionsPlugin,
            ILogger<VersionCacheCheckPlugin> logger,
            ILogger<VersionCacheManager> gitCommandLogger)
        {
            _configurationPlugin = configurationPlugin ?? throw new ArgumentNullException(nameof(configurationPlugin));
            _versionCacheOptionsPlugin = versionCacheOptionsPlugin ?? throw new ArgumentNullException(nameof(versionCacheOptionsPlugin));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _versionCacheManagerLogger = gitCommandLogger;
        }

        private void OnCreateVersionCacheManager()
        {
            var gitDirectory = _gitCommand.GetGitDirectory();

            var versionCacheDirectory = new VersionCacheDirectory(new VersionCacheDirectoryOptions(gitDirectory));

            _versionCacheManager = new VersionCacheManager(
                versionCacheDirectory,
                new VersionCacheEvaluator(),
                _versionCacheOptionsPlugin.CacheOptions,
                _versionCacheManagerLogger);

            _versionHashFile = new VersionHashFile(new VersionHashFileOptions(gitDirectory, _configFile), _versionCacheManager, versionCacheDirectory, _logger);

            Events.FireEvent(VersionCacheCheckEvents.CreatedVersionCacheManager, _versionCacheManager);
        }

        private void EnsureHavingVersionChecked()
        {
            if (!_isVersionChecked) {
                throw new InvalidOperationException("The version cache check has not been processed (If you depend on it you need to arrange your stuff after that check)");
            }
        }

        private void OnVersionCacheCheck()
        {
            if (!_versionHashFile.IsRecacheRequired(out var versionCache)) {
                _versionCache = versionCache;
            }

            _isVersionChecked = true;
            Events.FireEvent(VersionCacheCheckEvents.CheckedVersionCache);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.OnNextEvent(
                ConfigurationEvents.ConfiguredConfigurationBuilder,
                () => _configFile = _configurationPlugin.ConfigFile);

            Events.OnNextEvent(
                GitEvents.CreatedGitCommand,
                gitCommand => _gitCommand = gitCommand);

            Events.OnNextEvent(VersionCacheCheckEvents.CreateVersionCacheManager,
                OnCreateVersionCacheManager);

            Events.OnNextEvent(
                VersionCacheCheckEvents.CheckVersionCache,
                OnVersionCacheCheck);

            Events.OnNextEvent(GlobalServicesEvents.ConfigureServices,
                services => services.AddSingleton<IVersionCacheManager>(_versionCacheManager));
        }
    }
}
