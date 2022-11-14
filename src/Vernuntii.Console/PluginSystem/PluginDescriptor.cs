using System.Diagnostics.CodeAnalysis;
using System.Text;
using CSF.Collections;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The default plugin descriptor.
    /// </summary>
    public record PluginDescriptor
    {
        internal const int DefaultOrder = 0;

        private static readonly IEqualityComparer<IEnumerable<IPluginDependencyDescriptor>> _pluginDependencyDescriptorCollectionEqualityComparer = new ListEqualityComparer<IPluginDependencyDescriptor>();
        private static readonly ListEqualityComparer<int> _intEqaulityComparer = new ListEqualityComparer<int>();

        /// <summary>
        /// Describes an instantiated plugin whose service and implementation type is <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementation"></param>
        public static PluginDescriptor Create<TService>(TService implementation)
            where TService : IPlugin =>
            new PluginDescriptor(typeof(TService), implementation);

        /// <summary>
        /// Describes a not yet created plugin whose service and implementation type is <typeparamref name="TPlugin"/>.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        public static PluginDescriptor Create<TPlugin>()
            where TPlugin : IPlugin =>
            new PluginDescriptor(typeof(TPlugin), typeof(TPlugin));

        /// <summary>
        /// Describes a not yet created plugin whose service type is <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="implementationFactory"></param>
        public static PluginDescriptor Create<TService, TImplementation>(Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : IPlugin
            where TImplementation : class, TService =>
            new PluginDescriptor(typeof(TService), implementationFactory, typeof(TImplementation));

        /// <summary>
        /// Describes a not yet created plugin whose service and implementation type is <typeparamref name="TPlugin"/>.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="implementationFactory"></param>
        public static PluginDescriptor Create<TPlugin>(Func<IServiceProvider, TPlugin> implementationFactory)
            where TPlugin : class, IPlugin =>
            new PluginDescriptor(typeof(TPlugin), implementationFactory, typeof(TPlugin));

        /// <summary>
        /// The service type the plugin is associated with.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// The instantiated plugin.
        /// </summary>
        public IPlugin? Plugin { get; internal init; }

        /// <summary>
        /// The plugin type.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// The plugin factory.
        /// </summary>
        public Func<IServiceProvider, IPlugin>? ImplementationFactory { get; }

        /// <summary>
        /// Describes the plugins this plugin depends on.
        /// </summary>
        public IReadOnlyList<IPluginDependencyDescriptor> PluginDependencies { get; internal init; }

        /// <summary>
        /// The order of describing plugin. If not specified it is by default zero.
        /// If smaller than another plugin's order, then it will be executed before
        /// that plugin.
        /// </summary>
        public int PluginOrder {
            get => PluginOrders[0];
            init => PluginOrders[0] = value;
        }

        internal int[] PluginOrders { get; init; } = new[] { DefaultOrder };

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="implementationType"></param>
        /// <param name="implementation"></param>
        /// <param name="serviceType"></param>
        internal PluginDescriptor(Type serviceType, IPlugin? implementation, Type implementationType)
        {
            ImplementationType = implementationType;
            Plugin = implementation;
            ServiceType = serviceType;
            PluginDependencies = Array.Empty<IPluginDependencyDescriptor>();
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="implementationType"></param>
        /// <param name="serviceType"></param>
        /// <param name="implementationFactory"></param>
        internal PluginDescriptor(Type serviceType, Func<IServiceProvider, IPlugin>? implementationFactory, Type implementationType)
        {
            ImplementationType = implementationType;
            ServiceType = serviceType;
            ImplementationFactory = implementationFactory;
            PluginDependencies = Array.Empty<IPluginDependencyDescriptor>();
        }

        /// <summary>
        /// Describes an instantiated plugin whose service type is <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="implementation"></param>
        /// <param name="serviceType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public PluginDescriptor(Type serviceType, IPlugin implementation)
            : this(serviceType, implementation ?? throw new ArgumentNullException(nameof(implementation)), implementation.GetType())
        {
        }

        /// <summary>
        /// Describes an not yet created plugin whose service type is <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="implementationType"></param>
        /// <param name="serviceType"></param>
        public PluginDescriptor(Type serviceType, Type implementationType)
            : this(serviceType, implementation: null, implementationType)
        {
        }

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] PluginDescriptor? other) =>
            other is not null
            && ServiceType == other.ServiceType
            && EqualityComparer<IPlugin>.Default.Equals(Plugin, other.Plugin)
            && ImplementationType == other.ImplementationType
            && _pluginDependencyDescriptorCollectionEqualityComparer.Equals(PluginDependencies, other.PluginDependencies)
            && PluginOrders.SequenceEqual(other.PluginOrders);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(
            ServiceType,
            Plugin is null ? 0 : EqualityComparer<IPlugin>.Default.GetHashCode(Plugin),
            ImplementationType,
            _pluginDependencyDescriptorCollectionEqualityComparer.GetHashCode(PluginDependencies),
            _intEqaulityComparer.GetHashCode(PluginOrders));

        /// <inheritdoc/>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            PrintMembers(stringBuilder);
            return $"[{PluginOrder}, {stringBuilder}]";
        }
    }
}
