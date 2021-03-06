using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Events;
using Vernuntii.SemVer;
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
        public readonly static SubjectEvent<IServiceCollection> ConfigureGlobalServices = new SubjectEvent<IServiceCollection>();

        /// <summary>
        /// Event when global service collection has been configured.
        /// </summary>
        public readonly static SubjectEvent<IServiceCollection> ConfiguredGlobalServices = new SubjectEvent<IServiceCollection>();

        /// <summary>
        /// Event when calculation service collection is created.
        /// </summary>
        public readonly static SubjectEvent<IServiceCollection> CreatedCalculationServices = new SubjectEvent<IServiceCollection>();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public readonly static SubjectEvent<IServiceCollection> ConfiguredCalculationServices = new SubjectEvent<IServiceCollection>();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public readonly static SubjectEvent<IServiceProvider> CreatedCalculationServiceProvider = new SubjectEvent<IServiceProvider>();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public readonly static SubjectEvent<IVersionCache> CalculatedNextVersion = new SubjectEvent<IVersionCache>();


    }
}
