namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The options for cache provider.
    /// </summary>
    public class VersionCacheOptions : IVersionCacheOptions
    {
        /// <summary>
        /// Default internal cache id.
        /// </summary>
        public const string DefaultInternalCacheId = "SHORT_LIVING_CACHE";

        /// <summary>
        /// Default internal cache last access retention time.
        /// </summary>
        public readonly static TimeSpan DefaultInternalCacheLastAccessRetentionTime = TimeSpan.FromSeconds(15);

        /// <inheritdoc/>
        public string InternalCacheId { get; set; } = DefaultInternalCacheId;

        /// <inheritdoc/>
        public TimeSpan InternalCacheLastAccessRetentionTime { get; set; } = DefaultInternalCacheLastAccessRetentionTime;

        /// <inheritdoc/>
        public TimeSpan CacheCreationRetentionTime { get; set; } = TimeSpan.FromHours(2);

        /// <inheritdoc/>
        public TimeSpan? LastAccessRetentionTime { get; set; }

        /// <inheritdoc/>
        public string? CacheId { get; set; }

        /// <inheritdoc/>
        public bool EmptyCaches { get; set; }
    }
}
