using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.PluginSystem
{
    internal class PluginDescriptorCollection : IEnumerable<PluginDescriptor>
    {
        private static PluginDescriptor SupplementPluginDescriptor(PluginDescriptor pluginDescriptor)
        {
            var pluginDependencyDescriptors = FindOpenGenericAttributes<IPluginDependencyDescriptor>(typeof(PluginDependencyAttribute<>), inherit: false);

            pluginDescriptor = pluginDescriptor with {
                PluginDependencies = pluginDependencyDescriptors.ToList()
            };

            return pluginDescriptor;

            // TODO: .NET 7; use native GetCustomAttributes method
            IEnumerable<T> FindOpenGenericAttributes<T>(Type openGenericDefinition, bool inherit)
            {
                if (!openGenericDefinition.IsGenericTypeDefinition) {

                }

                var attributes = openGenericDefinition.GetCustomAttributes(inherit);

                return attributes
                    .Where(x => {
                        var attributeType = x.GetType();

                        return attributeType.IsGenericType
                            && attributeType.GetGenericTypeDefinition() == openGenericDefinition;
                    })
                    .Cast<T>();
            }
        }

        private List<PluginDescriptor> _pluginDescriptors;

        public PluginDescriptorCollection()
        {
            _pluginDescriptors = new List<PluginDescriptor>();
        }

        public void Add(PluginDescriptor pluginDescriptor)
        {
            var supplementedPluginDescriptor = SupplementPluginDescriptor(pluginDescriptor);
            _pluginDescriptors.Add(supplementedPluginDescriptor);
        }

        public IEnumerator<PluginDescriptor> GetEnumerator() =>
            _pluginDescriptors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            foreach (var pluginDescriptor in _pluginDescriptors) {
                IList<ServiceDescriptor> serviceList = services;
                serviceList.Add(pluginDescriptor.ToServiceDescriptor());
            }

            return services.BuildServiceProvider();
        }
    }
}
