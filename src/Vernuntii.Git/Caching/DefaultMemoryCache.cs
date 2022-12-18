namespace Vernuntii.Caching
{
    /// <summary>
    /// The default implementation of <see cref="IMemoryCache"/>.
    /// </summary>
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly Dictionary<object, object?> _dictinary = new();

        /// <inheritdoc/>
        public bool TryGetCache<T>(object key, out T value)
        {
            if (_dictinary.TryGetValue(key, out var valueObject)) {
                value = (T)valueObject!;
                return true;
            }

            value = default!;
            return false;
        }

        /// <inheritdoc/>
        public bool IsCached(object key) =>
            _dictinary.ContainsKey(key);

        /// <inheritdoc/>
        public void SetCache<T>(object key, T value) =>
            _dictinary[key] = value;

        /// <inheritdoc/>
        public void UnsetCache(object key) =>
            _dictinary.Remove(key);

        /// <inheritdoc/>
        public void UnsetCache() =>
            _dictinary.Clear();
    }
}
