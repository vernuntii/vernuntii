using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Reactive;
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
        public static readonly EventDiscriminator<IServiceCollection> ConfigureServices = EventDiscriminator.New<IServiceCollection>();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceProvider> CreatedScopedServiceProvider = EventDiscriminator.New<IServiceProvider>();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public static readonly EventDiscriminator<IVersionCache> CalculatedNextVersion = EventDiscriminator.New<IVersionCache>();


    }
}
