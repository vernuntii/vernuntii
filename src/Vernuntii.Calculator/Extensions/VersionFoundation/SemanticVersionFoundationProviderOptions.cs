namespace Vernuntii.Extensions.VersionFoundation
{
    /// <summary>
    /// The options for <see cref="SemanticVersionFoundationProvider"/>.
    /// </summary>
    public class SemanticVersionFoundationProviderOptions
    {
        /// <summary>
        /// Default internal cache id.
        /// </summary>
        public const string DefaultInternalCacheId = "SHORT_LIVING_CACHE";

        /// <summary>
        /// The internal cache id.
        /// </summary>
        public string InternalCacheId { get; } = DefaultInternalCacheId;

        /// <summary>
        /// Default internal cache last access retention time.
        /// </summary>
        public readonly static TimeSpan DefaultInternalCacheLastAccessRetentionTime = TimeSpan.FromSeconds(13);

        /// <summary>
        /// Default maximal cache creation retention time.
        /// </summary>
        public readonly static TimeSpan DefaultCacheCreationRetentionTime = TimeSpan.FromHours(2);

        /// <summary>
        /// Internal cache last access retention time.
        /// </summary>
        public TimeSpan InternalCacheLastAccessRetentionTime { get; set; } = DefaultInternalCacheLastAccessRetentionTime;

        /// <summary>
        /// Maximal cache creation retention time.
        /// </summary>
        public TimeSpan CacheCreationRetentionTime { get; set; } = DefaultCacheCreationRetentionTime;

        /// <summary>
        /// Indicates that the caches should be emptied where version informations are stored. This happens before the cache process itself.
        /// </summary>
        public bool EmptyCaches { get; set; }
    }
}
