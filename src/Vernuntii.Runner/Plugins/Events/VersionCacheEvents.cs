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
        /// Event when the version cache manager is going to be created.
        /// </summary>
        public static readonly EventDiscriminator<VersionCacheManagerContext> CreateVersionCacheManager = EventDiscriminator.New<VersionCacheManagerContext>();

        /// <summary>
        /// Event before up-to-date check.
        /// </summary>
        public static readonly EventDiscriminator CheckVersionCache = EventDiscriminator.New();

        /// <summary>
        /// Event after up-to-date check.
        /// </summary>
        public static readonly EventDiscriminator CheckedVersionCache = EventDiscriminator.New();
    }
}
