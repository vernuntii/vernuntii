using Microsoft.Extensions.DependencyInjection;

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
        /// Event when calculation service collection is created.
        /// </summary>
        public sealed class CreatedCalculationServices : PubSubEvent<CreatedCalculationServices, IServiceCollection>
        {
        }

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public sealed class CalculatedNextVersion : PubSubEvent<CalculatedNextVersion, SemanticVersion>
        {
        }
    }
}
