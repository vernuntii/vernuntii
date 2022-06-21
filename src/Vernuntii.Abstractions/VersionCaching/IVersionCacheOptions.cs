namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The options for cache provider.
    /// </summary>
    public interface IVersionCacheOptions
    {
        /// <summary>
        /// The internal cache id.
        /// </summary>
        string InternalCacheId { get; }

        /// <summary>
        /// Internal cache last access retention time.
        /// </summary>
        TimeSpan InternalCacheLastAccessRetentionTime { get; }

        /// <summary>
        /// Default maximal cache creation retention time.
        /// </summary>
        TimeSpan CacheCreationRetentionTime { get; }

        /// <summary>
        /// The last access retnention time.
        /// </summary>
        TimeSpan? LastAccessRetentionTime { get; }

        /// <summary>
        /// The cache id.
        /// </summary>
        string? CacheId { get; }

        /// <summary>
        /// Indicates that the caches should be emptied where version informations are stored. This happens before the cache process itself.
        /// </summary>
        bool EmptyCaches { get; }
    }
}
