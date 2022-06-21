using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.SemVer;
using Vernuntii.VersionCaching;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// Accesses an instance of version cache.
    /// </summary>
    public sealed class VersionRecacheIndicator : IVersionRecacheIndicator
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public readonly static VersionRecacheIndicator Default = new VersionRecacheIndicator();

        private static bool IsExpiredSinceCreation(DateTime expirationTime) =>
            DateTime.UtcNow > expirationTime;

        private static bool IsCacheExpiredSinceCreation2(IExpirableVersionCache versionCache) =>
            versionCache.ExpirationTime != null && IsExpiredSinceCreation(versionCache.ExpirationTime.Value);

        private static bool IsExpiredSinceLastAccess(DateTime? lastAccessTime, TimeSpan? retentionTime) =>
            lastAccessTime == null || DateTime.UtcNow > lastAccessTime + retentionTime;

        private static bool IsCacheExpiredSinceLastAccess2(IExpirableVersionCache versionCache, bool useLastAccessRetentionTime, TimeSpan? lastAccessRetentionTime) =>
            useLastAccessRetentionTime && IsExpiredSinceLastAccess(versionCache.LastAccessTime, lastAccessRetentionTime);

        /// <summary>
        /// Checks if cache expired since creation.
        /// </summary>
        /// <param name="versionCache"></param>
        public bool IsCacheExpiredSinceCreation([NotNullWhen(true)] IExpirableVersionCache? versionCache) =>
            versionCache != null && IsCacheExpiredSinceCreation2(versionCache);

        /// <summary>
        /// Checks if cache exoured since last access.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        public bool IsCacheExpiredSinceLastAccess(
            [NotNullWhen(true)] IExpirableVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime) =>
            versionCache != null
            && IsCacheExpiredSinceLastAccess2(versionCache, useLastAccessRetentionTime, lastAccessRetentionTime);

        /// <summary>
        /// Checks if cache is expired.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        /// <param name="recacheReason"></param>
        public bool IsRecacheRequired(
            [NotNullWhen(false)] IExpirableVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime,
            [NotNullWhen(true)] out string? recacheReason)
        {
            if (versionCache is null) {
                recacheReason = "Not cached yet";
            } else if (IsCacheExpiredSinceCreation(versionCache)) {
                recacheReason = "Expiration time";
            } else if (IsCacheExpiredSinceLastAccess(versionCache, useLastAccessRetentionTime, lastAccessRetentionTime)) {
                recacheReason = "Last access time";
            } else {
                recacheReason = null;
            }

            // If having any recache reason then return true.
            return recacheReason != null;
        }
    }
}
