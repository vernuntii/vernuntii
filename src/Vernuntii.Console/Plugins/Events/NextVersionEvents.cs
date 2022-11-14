using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionCaching;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="INextVersionPlugin"/>.
    /// </summary>
    public static class NextVersionEvents
    {
        /// <summary>
        /// Event when global service collection is about to be configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfigureGlobalServices = new();

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfiguredGlobalServices = new();

        /// <summary>
        /// Event when calculation service collection is created.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> CreatedCalculationServices = new();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfiguredCalculationServices = new();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly SubjectEvent<IServiceProvider> CreatedCalculationServiceProvider = new();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public static readonly SubjectEvent<IVersionCache> CalculatedNextVersion = new();


    }
}
