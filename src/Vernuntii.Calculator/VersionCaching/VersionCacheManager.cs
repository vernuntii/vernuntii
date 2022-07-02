using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vernuntii.Git;
using Vernuntii.Git.Command;
using Vernuntii.SemVer;
using Vernuntii.Text.Json;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// Manages the version cache.
    /// </summary>
    public class VersionCacheManager : IVersionCacheManager
    {
        /// <summary>
        /// The time to wait until lock attempt is aborted. Default are 10 seconds.
        /// </summary>
        private const int CacheLockAttemptSeconds = 10 * 1000;

        /// <inheritdoc/>
        public string CacheId { get; }

        private readonly IVersionCacheDirectory _cacheDirectory;
        private readonly TimeSpan? _creationRetentionTime;
        private readonly bool _useLastAccessRetentionTime;
        private readonly TimeSpan? _lastAccessRetentionTime;
        private readonly IVersionRecacheIndicator _recacheIndicator;
        private readonly bool _emptyCaches;
        private readonly ILogger<VersionCacheManager> _logger;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="cacheDirectory"></param>
        /// <param name="cacheOptions"></param>
        /// <param name="versionRecacheIndicator"></param>
        /// <param name="logger"></param>
        public VersionCacheManager(
            IVersionCacheDirectory cacheDirectory,
            IVersionRecacheIndicator versionRecacheIndicator,
            IVersionCacheOptions cacheOptions,
            ILogger<VersionCacheManager> logger)
        {
            TimeSpan? creationRetentionTime = cacheOptions.CacheCreationRetentionTime;

            var cacheId = cacheOptions.CacheId?.Trim();
            var lastAccessRetentionTime = cacheOptions.LastAccessRetentionTime;
            var useInternalCache = false;

            if (string.Equals(cacheId, cacheOptions.InternalCacheId, StringComparison.OrdinalIgnoreCase)) {
                logger.LogWarning(
                    "The versionCache id is equals to \"{InternalCacheId}\" and is reserved internally. The following will happen:" +
                    " 1. The creation retention time won't be used if specified." +
                    " 2. Last access retention time of {InternalCacheLastAccessRetentionTotalSeconds}s" +
                    " is used if not specified. If you use this versionCache id consciously then you can safely ignore this warning.",
                    cacheOptions.InternalCacheId,
                    cacheOptions.InternalCacheLastAccessRetentionTime.ToString("s\\.f", CultureInfo.InvariantCulture));

                useInternalCache = true;
            }

            if (string.IsNullOrEmpty(cacheId)) {
                cacheId = cacheOptions.InternalCacheId;
                useInternalCache = true;
            }

            if (useInternalCache) {
                creationRetentionTime = null;
                lastAccessRetentionTime ??= cacheOptions.InternalCacheLastAccessRetentionTime;
            }

            bool useLastAccessRetentionTime = lastAccessRetentionTime != null;

            _cacheDirectory = cacheDirectory;
            CacheId = cacheId;
            _creationRetentionTime = creationRetentionTime;
            _useLastAccessRetentionTime = useLastAccessRetentionTime;
            _lastAccessRetentionTime = lastAccessRetentionTime;
            _recacheIndicator = versionRecacheIndicator;
            _emptyCaches = cacheOptions.EmptyCaches;
            _logger = logger;
        }

        /// <inheritdoc/>
        private bool TryGetCache(
            [NotNullWhen(true)] out DefaultVersionCache? versionCache,
            out IManagedValueWriter<DefaultVersionCache> versionCacheWriter)
        {
            _cacheDirectory.CreateCacheDirectoryIfNotExisting();

            var cacheFileName = CacheId + ".data";
            var cacheFilePath = Path.Combine(_cacheDirectory.CacheDirectoryPath, cacheFileName);
            var cacheFile = new JsonFile<DefaultVersionCache>(cacheFilePath, CacheLockAttemptSeconds, VersionCacheSerializerContext.Default);

            //MaybeDeleteOtherCacheFiles(
            //    CacheDirectoryPath,
            //    skippableCacheFileName: cacheFileName);

            versionCacheWriter = cacheFile;

            try {
                if (cacheFile.TryReadValue(out versionCache)) {
                    return true;
                }

                return false;
            } catch {
                cacheFile.Dispose();
                throw;
            }
        }

        private RecacheIndicator GetRecacheIndicator()
        {
            _ = TryGetCache(
                out var versionCache,
                out var versionCacheWriter);

            try {
                return new RecacheIndicator(
                    versionCache,
                    versionCacheWriter,
                    _recacheIndicator.IsRecacheRequired(
                        versionCache,
                        _useLastAccessRetentionTime,
                        _lastAccessRetentionTime,
                        out var recacheReason),
                    recacheReason);
            } catch {
                versionCacheWriter?.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public bool IsRecacheRequired([NotNullWhen(false)] out IVersionCache? versionCache)
        {
            using var indicator = GetRecacheIndicator();

            if (indicator.IsRecacheRequired(out var concreteVersionCache, out var versionCacheWriter) is var isRecacheRequired
                && concreteVersionCache != null
                && _useLastAccessRetentionTime) {
                var newLastAccessTime = DateTime.UtcNow;
                concreteVersionCache.LastAccessTime = newLastAccessTime;

                _logger.LogInformation(
                    "Updated last access time of version cache to {LastAccessTime} (UTC)",
                    newLastAccessTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture));

                versionCacheWriter.Overwrite(concreteVersionCache);
            }

            versionCache = concreteVersionCache;
            return isRecacheRequired;
        }

        /// <inheritdoc/>
        public IVersionCache RecacheCache(ISemanticVersion newVersion, IBranch newBranch)
        {
            //if (_emptyCaches) {
            //    _cacheDirectory.DeleteCacheFiles();
            //    _logger.LogInformation("Emptied the caches where the version informations were stored");
            //}

            using var indicator = GetRecacheIndicator();

            if (indicator.IsRecacheRequired(out var versionCache, out var versionCacheWriter)) {
                versionCache = DefaultVersionCache.Create(newVersion, newBranch, _creationRetentionTime);

                if (_useLastAccessRetentionTime) {
                    versionCache.LastAccessTime = DateTime.UtcNow;
                }

                versionCacheWriter.Overwrite(versionCache);

                _logger.LogInformation(
                    "Updated cache with new version informations (Reason = {UpdateReason})",
                    indicator.RecacheReason);

                return versionCache;
            }

            return versionCache;
        }

        private class RecacheIndicator : IDisposable
        {
            private DefaultVersionCache? _versionCache;
            public string? RecacheReason { get; }

            private bool _isRecacheRequired;

            [MemberNotNullWhen(true, nameof(RecacheReason))]
            public bool IsRecacheRequired(
                [NotNullWhen(false)] out DefaultVersionCache? versionCache,
                out IValueWriter<DefaultVersionCache> versionCacheWriter)
            {
                versionCache = _versionCache;
                versionCacheWriter = _versionCacheWriter;
                return _isRecacheRequired;
            }

            private IManagedValueWriter<DefaultVersionCache> _versionCacheWriter;

            public RecacheIndicator(
                DefaultVersionCache? versionCache,
                IManagedValueWriter<DefaultVersionCache> disposableVersionCacheWriter,
                bool isRecacheRequired,
                string? recacheReason)
            {
                _versionCache = versionCache;
                _versionCacheWriter = disposableVersionCacheWriter ?? throw new ArgumentNullException(nameof(disposableVersionCacheWriter));
                _isRecacheRequired = isRecacheRequired;
                RecacheReason = recacheReason;
            }

            public void Dispose() => _versionCacheWriter.Dispose();
        }
    }
}
