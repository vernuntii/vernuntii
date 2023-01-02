using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using SoftCircuits.Collections;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistryFactory
    {
        private readonly PluginRegistrar _pluginRegistrar;

        public PluginRegistryFactory(PluginRegistrar pluginRegistrar) =>
            _pluginRegistrar = pluginRegistrar ?? throw new ArgumentNullException(nameof(pluginRegistrar));

        /// <summary>
        /// The plugin get built by the order of <paramref name="pluginDescriptors"/>.
        /// </summary>
        /// <param name="pluginDescriptors"></param>
        /// <param name="pluginProvider"></param>
        /// <returns>
        /// The type associated plugins. They keep the order of <paramref name="pluginDescriptors"/>.
        /// </returns>
        private IReadOnlyDictionary<Type, IPluginRegistration> BuildOrderedPluginRegistrations(
            IReadOnlySet<PluginDescriptor> pluginDescriptors,
            IServiceProvider pluginProvider)
        {
            var pluginRegistrationCounter = 0;
            var pluginRegistrations = new OrderedDictionary<Type, IPluginRegistration>();

            foreach (var pluginDescriptor in pluginDescriptors) {
                var plugin = (IPlugin)pluginProvider.GetRequiredService(pluginDescriptor.ServiceType);
                var pluginId = pluginRegistrationCounter++;
                var pluginRegistration = new PluginRegistration(plugin, pluginId, pluginDescriptor);
                pluginRegistrations.Add(pluginRegistration.ServiceType, pluginRegistration);
                //logger.LogTrace("Added plugin registration: {ServiceType} ({PluginType})", pluginRegistration.ServiceType, pluginRegistration.ImplementationType);
            }

            return new ReadOnlyDictionary<Type, IPluginRegistration>(pluginRegistrations);
        }

        public PluginRegistry Create(Action<IServiceCollection> postConfigureServices)
        {
            var lazyPluginRegistry = new LateBoundPluginRegistry(out var commitPluginRegistry);

            var pluginProvider = _pluginRegistrar.BuildServiceProvider(
                services => {
                    services.AddSingleton<IPluginRegistry>(lazyPluginRegistry);
                    postConfigureServices(services);
                },
                out var pluginDescriptors);

            var pluginRegistrations = BuildOrderedPluginRegistrations(
                pluginDescriptors,
                pluginProvider);

            var pluginRegistry = new PluginRegistry(pluginRegistrations, pluginProvider);
            commitPluginRegistry(pluginRegistry);
            return pluginRegistry;
        }
    }
}
