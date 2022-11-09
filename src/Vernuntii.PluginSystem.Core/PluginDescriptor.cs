using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The default plugin descriptor.
    /// </summary>
    public record PluginDescriptor : PluginDescriptorBase<IPlugin?>
    {
        /// <summary>
        /// Describes an instantiated plugin whose service and implementation type is <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="plugin"></param>
        public static PluginDescriptor Create<TService>(TService plugin)
            where TService : IPlugin =>
            new PluginDescriptor(typeof(TService), plugin);

        /// <summary>
        /// Describes a not yet created plugin whose service and implementation type is <typeparamref name="TPlugin"/>.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        public static PluginDescriptor Create<TPlugin>()
            where TPlugin : IPlugin =>
            new PluginDescriptor(typeof(TPlugin), typeof(TPlugin));

        /// <summary>
        /// Creates a copy of <paramref name="original"/>.
        /// </summary>
        /// <param name="original"></param>
        protected PluginDescriptor(PluginDescriptorBase<IPlugin?> original)
            : base(original)
        {
        }

        /// <summary>
        /// Describes an instantiated plugin whose service type is <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="serviceType"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public PluginDescriptor(Type serviceType, IPlugin plugin)
            : base(serviceType, plugin ?? throw new ArgumentNullException(nameof(plugin)), plugin.GetType())
        {
        }

        /// <summary>
        /// Describes an not yet created plugin whose service type is <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="serviceType"></param>
        public PluginDescriptor(Type serviceType, Type pluginType)
            : base(serviceType, plugin: null, pluginType)
        {
        }
    }
}
