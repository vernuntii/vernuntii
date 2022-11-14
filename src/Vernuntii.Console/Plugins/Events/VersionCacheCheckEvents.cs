using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionCaching;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IConfigurationPlugin"/>.
    /// </summary>
    public sealed class VersionCacheCheckEvents
    {
        /// <summary>
        /// Event when the version cache manager is about to be created.
        /// </summary>
        public static readonly SubjectEvent CreateVersionCacheManager = new();

        /// <summary>
        /// The event when the version cache manager has been created.
        /// </summary>
        public static readonly SubjectEvent<IVersionCacheManager> CreatedVersionCacheManager = new();

        /// <summary>
        /// Event before up-to-date check.
        /// </summary>
        public static readonly SubjectEvent CheckVersionCache = new();

        /// <summary>
        /// Event after up-to-date check.
        /// </summary>
        public static readonly SubjectEvent CheckedVersionCache = new();
    }
}
