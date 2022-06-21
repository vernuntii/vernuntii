using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// Accesses an instance of version cache.
    /// </summary>
    public interface IVersionRecacheIndicator
    {
        /// <summary>
        /// Checks if cache expired since creation.
        /// </summary>
        /// <param name="versionCache"></param>
        bool IsCacheExpiredSinceCreation([NotNullWhen(true)] IExpirableVersionCache? versionCache);

        /// <summary>
        /// Checks if cache exoured since last access.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        bool IsCacheExpiredSinceLastAccess(
            [NotNullWhen(true)] IExpirableVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime);

        /// <summary>
        /// Checks if cache is expired.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        /// <param name="recacheReason"></param>
        bool IsRecacheRequired(
            [NotNullWhen(false)] IExpirableVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime,
            [NotNullWhen(true)] out string? recacheReason);
    }
}
