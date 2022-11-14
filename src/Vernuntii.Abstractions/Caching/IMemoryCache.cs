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
        bool TryGetValue<T>(object key, out T value);

        /// <summary>
        /// Sets cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue<T>(object key, T value);

        /// <summary>
        /// Unsets cache.
        /// </summary>
        /// <param name="key"></param>
        void UnsetValue(object key);

        /// <summary>
        /// Clears cache.
        /// </summary>
        void Clear();
    }
}
