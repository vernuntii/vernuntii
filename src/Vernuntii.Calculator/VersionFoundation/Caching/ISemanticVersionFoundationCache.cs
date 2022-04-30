using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionFoundation.Caching
{
    /// <summary>
    /// The cache for the semantic version foundation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISemanticVersionFoundationCache<T>
        where T : class
    {
        /// <summary>
        /// Deletes cache files.
        /// </summary>
        /// <param name="gitDirectory"></param>
        void DeleteCacheFiles(string gitDirectory);

        /// <summary>
        /// Tries to get cache.
        /// </summary>
        /// <param name="gitDirectory"></param>
        /// <param name="cacheId"></param>
        /// <param name="presentationFoundation"></param>
        /// <param name="versionPresentationFoundationWriter"></param>
        /// <returns>True if cache exists</returns>
        bool TryGetCache(
            string gitDirectory,
            string cacheId,
            [NotNullWhen(true)] out T? presentationFoundation,
            out ISemanticVersionFoundationWriter<T> versionPresentationFoundationWriter);
    }
}
