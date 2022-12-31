using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IServicesPlugin"/>.
    /// </summary>
    public static class ServicesEvents
    {
        /// <summary>
        /// The event is called when the service provider is about to be created.
        /// </summary>
        public static readonly EventDiscriminator CreateServiceProvider = EventDiscriminator.New();

        /// <summary>
        /// The event is called when the service collection is to be configured.
        /// Called after <see cref="CreateServiceProvider"/> but before
        /// <see cref="CreatedServiceProvider"/>.
        /// </summary>
        public static readonly EventDiscriminator<IServiceCollection> ConfigureServices = EventDiscriminator.New<IServiceCollection>();

        /// <summary>
        /// The event is called when the service provider has been created.
        /// </summary>
        public static readonly EventDiscriminator<IServiceProvider> CreatedServiceProvider = EventDiscriminator.New<IServiceProvider>();
    }
}
