using Vernuntii.SemVer;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// Manages the version cache.
    /// </summary>
    public interface IVersionCacheManager : IVersionCacheReader
    {
        /// <summary>
        /// Gets or update cache.
        /// </summary>
        /// <param name="newVersion">The new version that is used when recaching.</param>
        /// <param name="versionCacheDataTuples">Cacheable data that is used when recaching.</param>
        /// <returns><see langword="true"/> when recache was happening</returns>
        IVersionCache RecacheCache(ISemanticVersion newVersion, IImmutableVersionCacheDataTuples versionCacheDataTuples);
    }
}
