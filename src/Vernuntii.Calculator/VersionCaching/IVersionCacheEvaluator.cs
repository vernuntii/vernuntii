using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// Accesses an instance of version cache.
    /// </summary>
    public interface IVersionCacheEvaluator
    {
        /// <summary>
        /// Checks if cache is expired.
        /// </summary>
        /// <param name="versionCache"></param>
        /// <param name="useLastAccessRetentionTime"></param>
        /// <param name="lastAccessRetentionTime"></param>
        /// <param name="comparableVersion">If not <see langword="null"/>, it will be used to be compared against cached version.</param>
        /// <param name="recacheReason"></param>
        bool IsRecacheRequired(
            [NotNullWhen(false)] IVersionCache? versionCache,
            bool useLastAccessRetentionTime,
            TimeSpan? lastAccessRetentionTime,
            ISemanticVersion? comparableVersion,
            [NotNullWhen(true)] out string? recacheReason);
    }
}
