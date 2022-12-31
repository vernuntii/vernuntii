using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IConfigurationPlugin"/>.
    /// </summary>
    public sealed class VersionCacheCheckEvents
    {
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
