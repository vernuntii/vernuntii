namespace Vernuntii.Caching
{
    /// <summary>
    /// Responsible to create a new memory cache.
    /// </summary>
    public interface IMemoryCacheFactory
    {
        /// <summary>
        /// Creates a memory cache.
        /// </summary>
        IMemoryCache Create();
    }
}
