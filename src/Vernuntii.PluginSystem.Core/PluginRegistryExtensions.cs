using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Extension methods for <see cref="IPluginRegistry"/>
    /// </summary>
    public static class PluginRegistryExtensions
    {
        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="pluginRegistry"></param>
        /// <param name="plugin"></param>
        public static void Register<TService>(this IPluginRegistry pluginRegistry, IPlugin plugin)
            where TService : IPlugin =>
            pluginRegistry.DescribePluginRegistration(new PluginDescriptor(typeof(TService), plugin));

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void Register<TService, TImplementation>(this IPluginRegistry pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService, new() =>
            pluginRegistry.DescribePluginRegistration(new PluginDescriptor(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void Register<TPlugin>(this IPluginRegistry pluginRegistry)
            where TPlugin : IPlugin, new() =>
            pluginRegistry.DescribePluginRegistration(PluginDescriptor.Create<TPlugin>());

        /// <summary>
        /// Tries to registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="pluginDescriptor"></param>
        private static void TryRegister(IPluginRegistry pluginRegistry, PluginDescriptor pluginDescriptor)
        {
            var pluginRegistration = pluginRegistry.PluginRegistrations.FirstOrDefault(x => x.ServiceType == pluginDescriptor.ServiceType);

            if (pluginRegistration is null) {
                pluginRegistry.DescribePluginRegistration(pluginDescriptor);
            }
        }

        /// <summary>
        /// Tries to registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        public static void TryRegister(this IPluginRegistry pluginRegistry, Type serviceType, IPlugin plugin) =>
            TryRegister(pluginRegistry, new PluginDescriptor(serviceType, plugin));

        /// <summary>
        /// Tries to registers a plugin.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="pluginRegistry"></param>
        /// <param name="plugin"></param>
        public static void TryRegister<TService>(this IPluginRegistry pluginRegistry, IPlugin plugin)
            where TService : IPlugin =>
            TryRegister(pluginRegistry, new PluginDescriptor(typeof(TService), plugin));

        /// <summary>
        /// Tries to registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void TryRegister<TService, TImplementation>(this IPluginRegistry pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService, new() =>
            TryRegister(pluginRegistry, new PluginDescriptor(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Tries to registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void TryRegister<TPlugin>(this IPluginRegistry pluginRegistry)
            where TPlugin : IPlugin, new() =>
            TryRegister(pluginRegistry, PluginDescriptor.Create<TPlugin>());
    }
}
