using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="IGitPlugin"/>.
    /// </summary>
    public static class GitEvents
    {
        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public sealed class ConfiguringGlobalServices : PubSubEvent<ConfiguringGlobalServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public sealed class ConfiguredGlobalServices : PubSubEvent<ConfiguredGlobalServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when calculation service collection is about to be configured.
        /// </summary>
        public sealed class ConfiguringCalculationServices : PubSubEvent<ConfiguringCalculationServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public sealed class ConfiguredCalculationServices : PubSubEvent<ConfiguredCalculationServices, IServiceCollection>
        {
        }
    }
}
