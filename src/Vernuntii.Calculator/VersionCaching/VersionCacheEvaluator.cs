using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionCaching
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

        private static bool IsExpiredSinceCreation(DateTime expirationTime) =>
            DateTime.UtcNow > expirationTime;

        private static bool IsExpiredSinceLastAccess(DateTime? lastAccessTime, TimeSpan? retentionTime) =>
            lastAccessTime == null || DateTime.UtcNow > lastAccessTime + retentionTime;

        /// <summary>
        /// Checks if cache expired since creation.
        /// </summary>
        /// <param name="versionCache"></param>
        private bool IsCacheExpiredSinceCreation([NotNullWhen(true)] IExpirableVersionCache? versionCache) =>
            versionCache != null
            && versionCache.ExpirationTime != null && IsExpiredSinceCreation(versionCache.ExpirationTime.Value);

        /// <summary>
        /// Checks if cache exoured since last access.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        private bool IsCacheExpiredSinceLastAccess(
            [NotNullWhen(true)] IExpirableVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime) =>
            versionCache != null
            && useLastAccessRetentionTime && IsExpiredSinceLastAccess(versionCache.LastAccessTime, lastAccessRetentionTime);

        /// <inheritdoc/>
        public bool IsRecacheRequired(
            [NotNullWhen(false)] IExpirableVersionCache? versionCache,
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
            } else if (!SemanticVersionComparer.VersionReleaseBuild.Equals(comparableVersion, versionCache.Version)) {
                recacheReason = "Version change";
            } else {
                recacheReason = null;
            }

            // If having any recache reason then return true.
            return recacheReason != null;
        }
    }
}
