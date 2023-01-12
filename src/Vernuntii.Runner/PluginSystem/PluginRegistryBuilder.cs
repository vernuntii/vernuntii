using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using SoftCircuits.Collections;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistryBuilder
    {
        private readonly List<Action<IServiceCollection>> _configurePluginServicesActions = new();

        /// <inheritdoc/>
        public void ConfigurePluginServices(Action<IServiceCollection> configure)
        {
            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            _configurePluginServicesActions.Add(configure);
        }

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

        public PluginRegistry Build(PluginRegistrar pluginRegistrar)
        {
            var lazyPluginRegistry = new LateBoundPluginRegistry(out var commitPluginRegistry);

            var pluginProvider = pluginRegistrar.BuildServiceProvider(
                services => {
                    services.AddSingleton<IPluginRegistry>(lazyPluginRegistry);

                    foreach (var configureServices in _configurePluginServicesActions) {
                        configureServices(services);
                    }
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
