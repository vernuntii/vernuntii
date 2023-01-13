using Vernuntii.PluginSystem.Reactive;
using Vernuntii.VersionPersistence;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IConfigurationPlugin"/>.
    /// </summary>
    public sealed class VersionCacheEvents
    {
        /// <summary>
        /// Event that may get emitted by third-party plugins signaling that the version cache needs to be checked.
        /// </summary>
        public static readonly EventDiscriminator CheckVersionCache = EventDiscriminator.New();

        /// <summary>
        /// Event when the version cache manager is going to be created.
        /// </summary>
        public static readonly EventDiscriminator<VersionCacheManagerContext> CreateVersionCacheManager = EventDiscriminator.New<VersionCacheManagerContext>();

        /// <summary>
        /// Event after up-to-date check.
        /// </summary>
        public static readonly EventDiscriminator OnCheckedVersionCache = EventDiscriminator.New();
    }
}
