namespace Vernuntii.Caching
{
    /// <summary>
    /// A memory cache.
    /// </summary>
    public interface IMemoryCache
    {
        /// <summary>
        /// Tries to get cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        bool TryGetCache<T>(object key, out T value);

        /// <summary>
        /// Tries to get cache.
        /// </summary>
        /// <param name="key"></param>
        bool IsCached(object key);

        /// <summary>
        /// Sets cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetCache<T>(object key, T value);

        /// <summary>
        /// Unsets cache.
        /// </summary>
        /// <param name="key"></param>
        void UnsetCache(object key);

        /// <summary>
        /// Clears all the cache.
        /// </summary>
        void UnsetCache();
    }
}
