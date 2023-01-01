using Vernuntii.Plugins.VersionPersistence;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events;

public class VersionCacheOptionsEvents
{
    /// <summary>
    /// Event before up-to-date check.
    /// </summary>
    public static readonly EventDiscriminator<VersionCacheOptions> ParsedVersionCacheOptions = EventDiscriminator.New<VersionCacheOptions>();
}
