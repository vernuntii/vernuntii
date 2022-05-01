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
        public static ValueTask<IPluginRegistration> RegisterAsync<TService>(this IPluginRegistry pluginRegistry, IPlugin plugin)
            where TService : IPlugin =>
            pluginRegistry.RegisterAsync(typeof(TService), plugin);

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static ValueTask<IPluginRegistration> RegisterAsync<TService, TImplementation>(this IPluginRegistry pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService, new() =>
            pluginRegistry.RegisterAsync(typeof(TService), new TImplementation());

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static ValueTask<IPluginRegistration> RegisterAsync<TPlugin>(this IPluginRegistry pluginRegistry)
            where TPlugin : IPlugin, new() =>
            pluginRegistry.RegisterAsync(typeof(IPlugin), new TPlugin());

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        public static async ValueTask<IPluginRegistration> TryRegisterAsync(this IPluginRegistry pluginRegistry, Type serviceType, IPlugin plugin)
        {
            var pluginRegistration = pluginRegistry.PluginRegistrations.FirstOrDefault(x => x.ServiceType == serviceType);

            if (pluginRegistration is null) {
                pluginRegistration = await pluginRegistry.RegisterAsync(serviceType, plugin);
            }

            return pluginRegistration;
        }

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="pluginRegistry"></param>
        /// <param name="plugin"></param>
        public static ValueTask<IPluginRegistration> TryRegisterAsync<TService>(this IPluginRegistry pluginRegistry, IPlugin plugin)
            where TService : IPlugin =>
            pluginRegistry.TryRegisterAsync(typeof(TService), plugin);

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static ValueTask<IPluginRegistration> TryRegisterAsync<TService, TImplementation>(this IPluginRegistry pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService, new() =>
            pluginRegistry.TryRegisterAsync(typeof(TService), new TImplementation());

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static ValueTask<IPluginRegistration> TryRegisterAsync<TPlugin>(this IPluginRegistry pluginRegistry)
            where TPlugin : IPlugin, new() =>
            pluginRegistry.TryRegisterAsync(typeof(TPlugin), new TPlugin());
    }
}
