using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="IVersioningPresetsPlugin"/>.
    /// </summary>
    public static class VersioningPresetsEvents
    {
        /// <summary>
        /// Event when globales services has been configured.
        /// </summary>
        public sealed class ConfiguredGlobalServices : PubSubEvent<ConfiguredGlobalServices, IServiceCollection>
        {
        }
    }
}
