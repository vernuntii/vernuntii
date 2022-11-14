using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Teronis.IO.FileLocking;
using Vernuntii.Cryptography;
using Vernuntii.Git.Commands;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionCaching;
using System.CommandLine;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using Vernuntii.PluginSystem.Meta;

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
        private bool isVersionChecked;

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
                new VersionRecacheIndicator(),
                _versionCacheOptionsPlugin.CacheOptions,
                _versionCacheManagerLogger);

            _versionHashFile = new VersionHashFile(new VersionHashFileOptions(gitDirectory, _configFile), _versionCacheManager, versionCacheDirectory, _logger);

            Events.Publish(VersionCacheCheckEvents.CreatedVersionCacheManager, _versionCacheManager);
        }

        private void EnsureHavingVersionChecked()
        {
            if (!isVersionChecked) {
                throw new InvalidOperationException("The version cache check has not been processed (If you depend on it you need to arrange your stuff after that check)");
            }
        }

        private void OnVersionCheck()
        {
            if (!_versionHashFile.IsRecacheRequired(out var versionCache)) {
                _versionCache = versionCache;
            }

            isVersionChecked = true;
            Events.Publish(VersionCacheCheckEvents.CheckedVersionCache);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.SubscribeOnce(
                ConfigurationEvents.ConfiguredConfigurationBuilder,
                () => _configFile = _configurationPlugin.ConfigFile);

            Events.SubscribeOnce(
                GitEvents.CreatedGitCommand,
                gitCommand => _gitCommand = gitCommand);

            Events.SubscribeOnce(VersionCacheCheckEvents.CreateVersionCacheManager,
                OnCreateVersionCacheManager);

            Events.SubscribeOnce(
                VersionCacheCheckEvents.CheckVersionCache,
                OnVersionCheck);

            Events.SubscribeOnce(GlobalServicesEvents.ConfigureServices,
                services => services.AddSingleton<IVersionCacheManager>(_versionCacheManager));
        }
    }
}
