using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.PluginSystem
{
    internal class PluginRegistrar : IPluginRegistrar
    {
        private static PluginDescriptor Supplement(PluginDescriptor pluginDescriptor)
        {
            var attributes = pluginDescriptor.ImplementationType.GetCustomAttributes(inherit: false);
            var pluginDependencyDescriptors = FindAttributes<IPluginDependencyDescriptor>();
            var pluginOrder = (attributes.SingleOrDefault(x => x.GetType() == typeof(PluginAttribute)) as PluginAttribute)?.Order ?? PluginDescriptor.DefaultOrder;

            pluginDescriptor = pluginDescriptor with {
                PluginDependencies = pluginDependencyDescriptors.ToList(),
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

        private IEnumerable<IPluginDependencyDescriptor> AddSupplemented(Queue<PluginDescriptor> supplementedPluginDescriptors)
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
            backlog.Enqueue(Supplement(pluginDescriptor));

            foreach (var pluginDependency in AddSupplemented(backlog)) {
                pluginDescriptorKey = pluginDependency.ServiceType;

                if (_uniquePluginDescriptors.ContainsKey(pluginDescriptorKey)) {
                    if (pluginDependency.TryRegister) {
                        continue;
                    }

                    throw new DuplicatePluginException();
                }

                backlog.Enqueue(Supplement(new PluginDescriptor(pluginDependency.ServiceType, pluginDependency.ImplementationType)));
            }
        }

        /// <summary>
        /// Builds a set that is ordered by <see cref="PluginDescriptor.PluginOrder"/>.
        /// </summary>
        internal IReadOnlySet<PluginDescriptor> BuildOrderlySet() =>
            _uniquePluginDescriptors.Values.ToImmutableSortedSet(PluginDescriptorOrderComparer.Default);

        /// <summary>
        /// Builds a service provider with the plugin descriptors that has been added so far.
        /// </summary>
        public ServiceProvider BuildServiceProvider(Action<IServiceCollection>? postConfigure, out IReadOnlySet<PluginDescriptor> orderlyPluginDescriptors)
        {
            var services = new ServiceCollection();
            orderlyPluginDescriptors = BuildOrderlySet();

            foreach (var pluginDescriptor in orderlyPluginDescriptors) {
                IList<ServiceDescriptor> serviceDescriptors = services;
                serviceDescriptors.Add(pluginDescriptor.ToServiceDescriptor());
            }

            postConfigure?.Invoke(services);
            return services.BuildServiceProvider();
        }
    }
}
