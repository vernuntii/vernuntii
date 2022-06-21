using System.Diagnostics.CodeAnalysis;
using Vernuntii.Git;
using Vernuntii.SemVer;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// Manages the version cache.
    /// </summary>
    public interface IVersionCacheManager
    {
        /// <summary>
        /// The cache identifier to differentiate between different setups.
        /// </summary>
        public string CacheId { get; }

        /// <summary>
        /// Checks whether recache is required.
        /// </summary>
        bool IsRecacheRequired([NotNullWhen(false)] out IVersionCache? versionCache);

        /// <summary>
        /// Gets or update cache.
        /// </summary>
        /// <param name="newVersion"></param>
        /// <param name="newBranch"></param>
        /// <returns><see langword="true"/> when recache was happening</returns>
        IVersionCache RecacheCache(ISemanticVersion newVersion, IBranch newBranch);
    }
}
