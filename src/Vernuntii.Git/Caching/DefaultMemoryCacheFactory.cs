namespace Vernuntii.Caching
{
    /// <summary>
    /// Defualt implementation of <see cref="IMemoryCacheFactory"/>.
    /// </summary>
    public class DefaultMemoryCacheFactory : IMemoryCacheFactory
    {
        /// <summary>
        /// The default instance of this type.
        /// </summary>
        public static readonly DefaultMemoryCacheFactory Default = new();

        /// <inheritdoc/>
        public IMemoryCache Create() => new DefaultMemoryCache();
    }
}
