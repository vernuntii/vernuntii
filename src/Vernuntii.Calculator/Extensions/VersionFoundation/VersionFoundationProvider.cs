using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vernuntii.Git;
using Vernuntii.VersionFoundation;
using Vernuntii.VersionFoundation.Caching;
using Teronis;
using Vernuntii.SemVer;

namespace Vernuntii.Extensions.VersionFoundation
{
    /// <summary>
    /// Provides an instance of <see cref="IVersionFoundation"/>.
    /// </summary>
    public class VersionFoundationProvider
    {
        private static bool IsCacheExpiredSinceLastAccess(DateTime? lastAccessTime, TimeSpan? retentionTime) =>
            lastAccessTime == null || DateTime.UtcNow > lastAccessTime + retentionTime;

        private static bool IsCacheExpiredSinceCreation(DateTime expirationTime) =>
            DateTime.UtcNow > expirationTime;

        private readonly VersionFoundationProviderOptions _options;
        private readonly IRepository _repository;
        private readonly ISemanticVersionFoundationCache<SemanticVersionFoundation> _versionFoundationCache;
        private readonly SlimLazy<ISingleVersionCalculation> _lazyCalculation;
        private readonly ILogger<VersionFoundationProvider> _logger;
        private readonly Action<ILogger, Exception?> _warnInternalCacheIdUsage;
        private readonly Action<ILogger, Exception?> _logCachesEmptied;
        private readonly Action<ILogger, string, Exception?> _logLastAccessUpdated;
        private readonly Action<ILogger, string, Exception?> _logCacheUpdated;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="repository"></param>
        /// <param name="versionFoundationCache"></param>
        /// <param name="lazyCalculation"></param>
        /// <param name="logger"></param>
        public VersionFoundationProvider(
            IOptions<VersionFoundationProviderOptions> options,
            IRepository repository,
            ISemanticVersionFoundationCache<SemanticVersionFoundation> versionFoundationCache,
            SlimLazy<ISingleVersionCalculation> lazyCalculation,
            ILogger<VersionFoundationProvider> logger)
        {
            _options = options.Value;
            _repository = repository;
            _versionFoundationCache = versionFoundationCache;
            _lazyCalculation = lazyCalculation;
            _logger = logger;

            _warnInternalCacheIdUsage = LoggerMessage.Define(
                LogLevel.Warning,
                new EventId(1),
                $"The cache id is equals to \"{_options.InternalCacheId}\" and is reserved internally. The following will happen:" +
                $" 1. The creation retention time won't be used if specified." +
                $" 2. Last access retention time of {_options.InternalCacheLastAccessRetentionTime.ToString("s\\.f", CultureInfo.InvariantCulture)}s" +
                $" is used if not specified. If you use this cache id consciously then you can safely ignore this warning.");

            _logCachesEmptied = LoggerMessage.Define(
                LogLevel.Information,
                new EventId(2),
                "Emptied the caches where the version informations were stored");

            _logLastAccessUpdated = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(3),
                "Updated last access time of cache to {LastAccessTime} (UTC)");

            _logCacheUpdated = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(4),
                "Updated cache with new version informations (Reason = {UpdateReason})");
        }

        private static bool ShouldUpdateCache(
            [NotNullWhen(false)] IVersionFoundation<SemanticVersion>? versionFoundation,
            string activeBranchCommitSha,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime,
            [NotNullWhen(true)] out string? cacheUpdateReason)
        {
            if (versionFoundation is null) {
                cacheUpdateReason = "Not cached yet";
            } else if (versionFoundation.CommitSha != activeBranchCommitSha) {
                cacheUpdateReason = "Branch changed";
            } else if (versionFoundation.ExpirationTime != null && IsCacheExpiredSinceCreation(versionFoundation.ExpirationTime.Value)) {
                cacheUpdateReason = "Expiration time";
            } else if (useLastAccessRetentionTime && IsCacheExpiredSinceLastAccess(versionFoundation.LastAccessTime, lastAccessRetentionTime)) {
                cacheUpdateReason = "Last access time";
            } else {
                cacheUpdateReason = null;
            }

            return cacheUpdateReason != null;
        }

        /// <summary>
        /// Gets the version foundation either cached or not.
        /// </summary>
        /// <param name="cacheId">The cache id.</param>
        /// <param name="creationRetentionTime">The cache retention time since creation.</param>
        /// <param name="lastAccessRetentionTime">The cache retention time since last access.</param>
        public IVersionFoundation GetFoundation(string? cacheId, TimeSpan? creationRetentionTime, TimeSpan? lastAccessRetentionTime)
        {
            if (creationRetentionTime == null) {
                creationRetentionTime = _options.CacheCreationRetentionTime;
            }

            cacheId = cacheId?.Trim();
            ISemanticVersionFoundationWriter<SemanticVersionFoundation> presentationFoundationWriter;
            var useInternalCache = false;

            if (string.Equals(cacheId, _options.InternalCacheId, StringComparison.OrdinalIgnoreCase)) {
                _warnInternalCacheIdUsage?.Invoke(_logger, null);
                useInternalCache = true;
            }

            if (string.IsNullOrEmpty(cacheId)) {
                cacheId = _options.InternalCacheId;
                useInternalCache = true;
            }

            if (useInternalCache) {
                creationRetentionTime = null;
                lastAccessRetentionTime ??= _options.InternalCacheLastAccessRetentionTime;
            }

            bool useLastAccessRetentionTime = lastAccessRetentionTime != null;
            var gitDirectory = _repository.GetGitDirectory();

            if (_options.EmptyCaches) {
                _versionFoundationCache.DeleteCacheFiles(gitDirectory);
                _logCachesEmptied.Invoke(_logger, null);
            }

            _ = _versionFoundationCache.TryGetCache(
                gitDirectory,
                cacheId,
                out var versionFoundation,
                out presentationFoundationWriter);

            try {
                var activeBranch = _repository.GetActiveBranch();

                if (ShouldUpdateCache(
                        versionFoundation,
                        activeBranch.CommitSha,
                        useLastAccessRetentionTime,
                        lastAccessRetentionTime,
                        out var cacheUpdateReason)) {
                    var calculatedVersion = _lazyCalculation.Value.GetVersion();
                    versionFoundation = SemanticVersionFoundation.Create(calculatedVersion, activeBranch, creationRetentionTime);

                    if (useLastAccessRetentionTime) {
                        versionFoundation.LastAccessTime = DateTime.UtcNow;
                    }

                    presentationFoundationWriter.WriteVersionFoundation(versionFoundation);
                    _logCacheUpdated.Invoke(_logger, cacheUpdateReason, null);
                } else if (useLastAccessRetentionTime) {
                    var newLastAccessTime = DateTime.UtcNow;
                    versionFoundation.LastAccessTime = newLastAccessTime;
                    _logLastAccessUpdated(_logger, newLastAccessTime.ToString(@"HH\:mm\:ss", CultureInfo.InvariantCulture), null);
                    presentationFoundationWriter.WriteVersionFoundation(versionFoundation);
                }
            } finally {
                presentationFoundationWriter.Dispose();
            }

            return versionFoundation;
        }
    }
}
