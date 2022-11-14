using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Autofac;
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
        public readonly static SubjectEvent CreateServiceProvider = new SubjectEvent();

        /// <summary>
        /// The event is called when the service collection is to be configured.
        /// Called after <see cref="CreateServiceProvider"/> but before
        /// <see cref="CreatedServiceProvider"/>.
        /// </summary>
        public readonly static SubjectEvent<IServiceCollection> ConfigureServices = new SubjectEvent<IServiceCollection>();

        /// <summary>
        /// The event is called when the service provider has been created.
        /// </summary>
        public readonly static SubjectEvent<ILifetimeScopedServiceProvider> CreatedServiceProvider = new SubjectEvent<ILifetimeScopedServiceProvider>();
    }
}
