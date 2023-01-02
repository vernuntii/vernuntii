using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistrar : IPluginRegistrar
    {
        private static PluginDescriptor SupplementPluginDescriptor(PluginDescriptor pluginDescriptor)
        {
            var attributes = pluginDescriptor.ImplementationType.GetCustomAttributes(inherit: false);
            var pluginDependencies = FindAttributes<IPluginDependencyDescriptor>();
            var pluginOrder = (attributes.SingleOrDefault(x => x.GetType() == typeof(PluginAttribute)) as PluginAttribute)?.Order ?? PluginDescriptor.DefaultOrder;

            pluginDescriptor = pluginDescriptor with {
                PluginDependencies = pluginDependencies.ToList(),
                PluginOrder = pluginOrder
            };

            return pluginDescriptor;

            // TODO: .NET 8; use native GetCustomAttributes method
            IEnumerable<TBase> FindAttributes<TBase>()
            {
                var baseType = typeof(TBase);

                return attributes
                    .Where(x => x.GetType().IsAssignableTo(baseType))
                    .Cast<TBase>();
            }
        }

        public IEnumerable<PluginDescriptor> PluginDescriptors => _uniquePluginDescriptors.Values;

        private readonly Dictionary<Type, PluginDescriptor> _uniquePluginDescriptors;

        public PluginRegistrar() =>
            _uniquePluginDescriptors = new Dictionary<Type, PluginDescriptor>();

        private IEnumerable<IPluginDependencyDescriptor> EnumerateSupplementedPluginDescriptors(Queue<PluginDescriptor> supplementedPluginDescriptors)
        {
            while (supplementedPluginDescriptors.TryDequeue(out var next)) {
                _uniquePluginDescriptors.Add(next.ServiceType, next);

                foreach (var dependency in next.PluginDependencies) {
                    yield return dependency;
                }
            }
        }

        public void Add(PluginDescriptor pluginDescriptor)
        {
            var pluginDescriptorKey = pluginDescriptor.ServiceType;

            if (_uniquePluginDescriptors.ContainsKey(pluginDescriptorKey)) {
                throw new DuplicatePluginException();
            }

            var backlog = new Queue<PluginDescriptor>();
            backlog.Enqueue(SupplementPluginDescriptor(pluginDescriptor));

            foreach (var pluginDependency in EnumerateSupplementedPluginDescriptors(backlog)) {
                pluginDescriptorKey = pluginDependency.ServiceType;

                if (_uniquePluginDescriptors.ContainsKey(pluginDescriptorKey)) {
                    if (pluginDependency.TryRegister) {
                        continue;
                    }

                    throw new DuplicatePluginException($"The '{pluginDependency.ServiceType}->{pluginDependency.ImplementationType}' plugin already exists");
                }

                backlog.Enqueue(SupplementPluginDescriptor(new PluginDescriptor(pluginDependency.ServiceType, pluginDependency.ImplementationType)));
            }
        }

        // TODO: internal->private via IoC (see dependent test cases)
        /// <summary>
        /// Builds a set that is ordered by <see cref="PluginDescriptor.PluginOrder"/>.
        /// </summary>
        internal IReadOnlySet<PluginDescriptor> BuildOrderedSet() =>
            _uniquePluginDescriptors.Values.ToImmutableSortedSet(PluginDescriptorOrderComparer.Default);

        /// <summary>
        /// Builds a service provider with the plugin descriptors that has been added so far.
        /// </summary>
        /// <param name="postConfigure"></param>
        /// <param name="pluginDescriptors">The plugin descriptors ordered by their individual order.</param>
        public ServiceProvider BuildServiceProvider(Action<IServiceCollection>? postConfigure, out IReadOnlySet<PluginDescriptor> pluginDescriptors)
        {
            var services = new ServiceCollection();
            pluginDescriptors = BuildOrderedSet();

            foreach (var pluginDescriptor in pluginDescriptors) {
                IList<ServiceDescriptor> serviceDescriptors = services;
                serviceDescriptors.Add(pluginDescriptor.ToServiceDescriptor());
            }

            postConfigure?.Invoke(services);
            return services.BuildServiceProvider();
        }
    }
}
