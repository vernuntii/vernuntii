using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem
{
    internal static class PluginDescriptorExtensions
    {
        public static ServiceDescriptor ToServiceDescriptor(this PluginDescriptor pluginDescriptor)
        {
            if (pluginDescriptor.Plugin is not null) {
                return new ServiceDescriptor(pluginDescriptor.ServiceType, pluginDescriptor.Plugin);
            }

            if (pluginDescriptor.ImplementationFactory is not null) {
                return new ServiceDescriptor(pluginDescriptor.ServiceType, pluginDescriptor.ImplementationFactory, ServiceLifetime.Singleton);
            }

            return new ServiceDescriptor(pluginDescriptor.ServiceType, pluginDescriptor.ImplementationType, ServiceLifetime.Singleton);
        }
    }
}
