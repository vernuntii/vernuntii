using Vernuntii.VersionPersistence;

namespace Vernuntii.Plugins.VersionPersistence
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
        public static readonly TimeSpan DefaultInternalCacheLastAccessRetentionTime = TimeSpan.FromSeconds(15);

        public static readonly TimeSpan DefaultCacheCreationRetentionTime = TimeSpan.FromHours(2);

        /// <inheritdoc/>
        public string InternalCacheId { get; set; } = DefaultInternalCacheId;

        /// <inheritdoc/>
        public TimeSpan InternalCacheLastAccessRetentionTime { get; set; } = DefaultInternalCacheLastAccessRetentionTime;

        /// <inheritdoc/>
        public TimeSpan CacheCreationRetentionTime { get; set; } = DefaultCacheCreationRetentionTime;

        /// <inheritdoc/>
        public TimeSpan? LastAccessRetentionTime { get; set; }

        /// <inheritdoc/>
        public string? CacheId { get; set; }

        /// <inheritdoc/>
        public bool EmptyCaches { get; set; }
    }
}
