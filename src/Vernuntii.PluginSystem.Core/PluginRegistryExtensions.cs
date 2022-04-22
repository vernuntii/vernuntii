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
        public static IPluginRegistration Register<TService>(this IPluginRegistry pluginRegistry, IPlugin plugin)
            where TService : IPlugin =>
            pluginRegistry.Register(typeof(TService), plugin);

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static IPluginRegistration Register<TService, TImplementation>(this IPluginRegistry pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService, new() =>
            pluginRegistry.Register(typeof(TService), new TImplementation());

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static IPluginRegistration Register<TPlugin>(this IPluginRegistry pluginRegistry)
            where TPlugin : IPlugin, new() =>
            pluginRegistry.Register(typeof(IPlugin), new TPlugin());

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        public static IPluginRegistration TryRegister(this IPluginRegistry pluginRegistry, Type serviceType, IPlugin plugin) =>
            pluginRegistry.PluginRegistrations.FirstOrDefault(x => x.ServiceType == serviceType) ?? pluginRegistry.Register(serviceType, plugin);

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="pluginRegistry"></param>
        /// <param name="plugin"></param>
        public static IPluginRegistration TryRegister<TService>(this IPluginRegistry pluginRegistry, IPlugin plugin)
            where TService : IPlugin =>
            pluginRegistry.TryRegister(typeof(TService), plugin);

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static IPluginRegistration TryRegister<TService, TImplementation>(this IPluginRegistry pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService, new() =>
            pluginRegistry.TryRegister(typeof(TService), new TImplementation());

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static IPluginRegistration TryRegister<TPlugin>(this IPluginRegistry pluginRegistry)
            where TPlugin : IPlugin, new() =>
            pluginRegistry.TryRegister(typeof(TPlugin), new TPlugin());
    }
}
