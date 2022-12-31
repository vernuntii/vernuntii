using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using SoftCircuits.Collections;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistryFactory
    {
        private readonly PluginRegistrar _pluginServiceProviderBuilder;

        public PluginRegistryFactory(PluginRegistrar pluginServiceProviderBuilder) =>
            _pluginServiceProviderBuilder = pluginServiceProviderBuilder ?? throw new ArgumentNullException(nameof(pluginServiceProviderBuilder));

        private IReadOnlyDictionary<Type, IPluginRegistration> BuildOrderlyPluginRegistrations(
            IReadOnlySet<PluginDescriptor> orderlyPluginDescriptors,
            IServiceProvider pluginProvider)
        {
            var pluginRegistrationCounter = 0;
            var orderlyPluginRegistrations = new OrderedDictionary<Type, IPluginRegistration>();

            foreach (var pluginDescriptor in orderlyPluginDescriptors) {
                var plugin = (IPlugin)pluginProvider.GetRequiredService(pluginDescriptor.ServiceType);
                var pluginId = pluginRegistrationCounter++;
                var pluginRegistration = new PluginRegistration(plugin, pluginId, pluginDescriptor);
                orderlyPluginRegistrations.Add(pluginRegistration.ServiceType, pluginRegistration);
                //logger.LogTrace("Added plugin registration: {ServiceType} ({PluginType})", pluginRegistration.ServiceType, pluginRegistration.ImplementationType);
            }

            return new ReadOnlyDictionary<Type, IPluginRegistration>(orderlyPluginRegistrations);
        }

        public PluginRegistry Create(Action<IServiceCollection> postConfigureServices)
        {
            var lazyPluginRegistry = new LateBoundPluginRegistry(out var commitPluginRegistry);

            var pluginProvider = _pluginServiceProviderBuilder.BuildServiceProvider(
                services => {
                    services.AddSingleton<IPluginRegistry>(lazyPluginRegistry);
                    postConfigureServices(services);
                },
                out var orderlyPluginDescriptors);

            var orderlyPluginRegistrations = BuildOrderlyPluginRegistrations(
                orderlyPluginDescriptors,
                pluginProvider);

            var pluginRegistry = new PluginRegistry(orderlyPluginRegistrations, pluginProvider);
            commitPluginRegistry(pluginRegistry);
            return pluginRegistry;
        }
    }
}
