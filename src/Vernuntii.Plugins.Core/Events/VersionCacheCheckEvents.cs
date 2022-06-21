using Microsoft.Extensions.Configuration;
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
        public readonly static SubjectEvent CreateVersionCacheManager = new SubjectEvent();

        /// <summary>
        /// The event when the version cache manager has been created.
        /// </summary>
        public readonly static SubjectEvent<IVersionCacheManager> CreatedVersionCacheManager = new SubjectEvent<IVersionCacheManager>();

        /// <summary>
        /// Event before up-to-date check.
        /// </summary>
        public readonly static SubjectEvent CheckVersionCache = new SubjectEvent();

        /// <summary>
        /// Event after up-to-date check.
        /// </summary>
        public readonly static SubjectEvent CheckedVersionCache = new SubjectEvent();
    }
}
