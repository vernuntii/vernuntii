using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IGlobalServicesPlugin"/>.
    /// </summary>
    public static class GlobalServicesEvents
    {
        /// <summary>
        /// The event is called when the service provider is about to be created.
        /// </summary>
        public static readonly SubjectEvent CreateServiceProvider = new();

        /// <summary>
        /// The event is called when the service collection is to be configured.
        /// Called after <see cref="CreateServiceProvider"/> but before
        /// <see cref="CreatedServiceProvider"/>.
        /// </summary>
        public static readonly SubjectEvent<IServiceCollection> ConfigureServices = new();

        /// <summary>
        /// The event is called when the service provider has been created.
        /// </summary>
        public static readonly SubjectEvent<IServiceProvider> CreatedServiceProvider = new();
    }
}
