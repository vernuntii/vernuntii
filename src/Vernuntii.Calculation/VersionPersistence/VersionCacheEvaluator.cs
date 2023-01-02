using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// Accesses an instance of version cache.
    /// </summary>
    public sealed class VersionCacheEvaluator : IVersionCacheEvaluator
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public static readonly VersionCacheEvaluator Default = new();

        private static bool IsExpiredSinceLastAccess(DateTime? lastAccessTime, TimeSpan? retentionTime) =>
            lastAccessTime == null || DateTime.UtcNow > lastAccessTime + retentionTime;

        /// <summary>
        /// Checks if cache expired since creation.
        /// </summary>
        /// <param name="versionCache"></param>
        private static bool IsCacheExpiredSinceCreation([NotNullWhen(true)] IVersionCache? versionCache) =>
            versionCache != null
            && versionCache.ExpirationTime.IsExpired();

        /// <summary>
        /// Checks if cache exoured since last access.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        private static bool IsCacheExpiredSinceLastAccess(
            IVersionCache versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime) =>
            useLastAccessRetentionTime && IsExpiredSinceLastAccess(versionCache.LastAccessTime, lastAccessRetentionTime);

        private static bool IsCacheRequiredBecauseMismatchingVersion(
            IVersionCache versionCache,
            ISemanticVersion? comparableVersion) =>
            comparableVersion != null && !SemanticVersionComparer.VersionReleaseBuild.Equals(comparableVersion, versionCache.Version);

        /// <inheritdoc/>
        public bool IsRecacheRequired(
            [NotNullWhen(false)] IVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime,
            ISemanticVersion? comparableVersion,
            [NotNullWhen(true)] out string? recacheReason)
        {
            if (versionCache is null) {
                recacheReason = "Not cached yet";
            } else if (IsCacheExpiredSinceCreation(versionCache)) {
                recacheReason = "Expiration time";
            } else if (IsCacheExpiredSinceLastAccess(versionCache, useLastAccessRetentionTime, lastAccessRetentionTime)) {
                recacheReason = "Last access time";
            } else if (IsCacheRequiredBecauseMismatchingVersion(versionCache, comparableVersion)) {
                recacheReason = "Version mismatch";
            } else {
                recacheReason = null;
            }

            // If having any recache reason then return true.
            return recacheReason != null;
        }
    }
}
