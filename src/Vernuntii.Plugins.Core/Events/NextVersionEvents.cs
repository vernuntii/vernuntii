using Microsoft.Extensions.DependencyInjection;
using Vernuntii.SemVer;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="INextVersionPlugin"/>.
    /// </summary>
    public static class NextVersionEvents
    {
        /// <summary>
        /// Event when global service collection is created.
        /// </summary>
        public sealed class CreatedGlobalServices : PubSubEvent<CreatedGlobalServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public sealed class ConfiguredGlobalServices : PubSubEvent<ConfiguredGlobalServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when calculation service collection is created.
        /// </summary>
        public sealed class CreatedCalculationServices : PubSubEvent<CreatedCalculationServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public sealed class ConfiguredCalculationServices : PubSubEvent<ConfiguredCalculationServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public sealed class CreatedCalculationServiceProvider : PubSubEvent<CreatedCalculationServiceProvider, IServiceProvider>
        {
        }

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public sealed class CalculatedNextVersion : PubSubEvent<CalculatedNextVersion, ISemanticVersion>
        {
        }
    }
}
