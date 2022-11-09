using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            return new ServiceDescriptor(pluginDescriptor.ServiceType, pluginDescriptor.PluginType, ServiceLifetime.Singleton);
        }
    }
}
