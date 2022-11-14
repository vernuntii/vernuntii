namespace Vernuntii.Caching
{
    /// <summary>
    /// The default implementation of <see cref="IMemoryCache"/>.
    /// </summary>
    public class DefaultMemoryCache : IMemoryCache
    {
        private readonly Dictionary<object, object?> _dictinary = new();

        /// <inheritdoc/>
        public bool TryGetValue<T>(object key, out T value)
        {
            if (_dictinary.TryGetValue(key, out var valueObject)) {
                value = (T)valueObject!;
                return true;
            }

            value = default!;
            return false;
        }

        /// <inheritdoc/>
        public void SetValue<T>(object key, T value) =>
            _dictinary[key] = value;

        /// <inheritdoc/>
        public void UnsetValue(object key) =>
            _dictinary.Remove(key);

        /// <inheritdoc/>
        public void Clear() =>
            _dictinary.Clear();
    }
}
