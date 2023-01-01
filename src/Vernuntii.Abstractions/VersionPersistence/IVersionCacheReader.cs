using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// Manages the version cache.
    /// </summary>
    public interface IVersionCacheReader
    {
        /// <summary>
        /// The cache identifier to differentiate between different setups.
        /// </summary>
        public string CacheId { get; }

        /// <summary>
        /// Checks whether recache is required.
        /// </summary>
        bool IsRecacheRequired([NotNullWhen(false)] out IVersionCache? versionCache);
    }
}
