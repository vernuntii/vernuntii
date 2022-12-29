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
        public static readonly EventDiscriminator<IServiceCollection> ConfigureServices = new();

        /// <summary>
        /// Event when calculation service collection has been configured.
        /// </summary>
        public static readonly EventDiscriminator<IServiceProvider> CreatedScopedServiceProvider = new();

        /// <summary>
        /// Event when next version has been calculated.
        /// </summary>
        public static readonly EventDiscriminator<IVersionCache> CalculatedNextVersion = new();


    }
}
