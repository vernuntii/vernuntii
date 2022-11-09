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
    [PluginDependency<VersionCacheOptionsPlugin>(TryRegister = true)]
    public class VersionCacheCheckPlugin : Plugin, IVersionCacheCheckPlugin
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

        private IConfigurationPlugin _configurationPlugin = null!;
        private ILoggingPlugin _loggerPlugin = null!;
        private IGitCommand _gitCommand = null!;
        private string? _configFile;
        private ILogger _logger = null!;
        private VersionCacheOptionsPlugin _versionCacheOptionsPlugin = null!;
        private VersionCacheManager _versionCacheManager = null!;
        private VersionHashFile _versionHashFile = null!;
        private IVersionCache? _versionCache;
        private bool isVersionChecked;

        private void OnCreateVersionCacheManager()
        {
            var gitDirectory = _gitCommand.GetGitDirectory();

            var versionCacheDirectory = new VersionCacheDirectory(new VersionCacheDirectoryOptions(gitDirectory));

            _versionCacheManager = new VersionCacheManager(
                versionCacheDirectory,
                new VersionRecacheIndicator(),
                _versionCacheOptionsPlugin.CacheOptions,
                _loggerPlugin.CreateLogger<VersionCacheManager>());

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
            _configurationPlugin = Plugins.GetPlugin<IConfigurationPlugin>();
            _loggerPlugin = Plugins.GetPlugin<ILoggingPlugin>();
            _logger = _loggerPlugin.CreateLogger<VersionCacheCheckPlugin>();
            _versionCacheOptionsPlugin = Plugins.GetPlugin<VersionCacheOptionsPlugin>();

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
