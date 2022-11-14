namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Extension methods for <see cref="IPluginProviderBuilder"/>
    /// </summary>
    public static class PluginProviderBuilderExtensions
    {
        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="builder"></param>
        /// <param name="plugin"></param>
        public static void Add<TService>(this IPluginProviderBuilder builder, IPlugin plugin)
            where TService : IPlugin =>
            builder.Add(new PluginDescriptor(typeof(TService), plugin));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <param name="builder"></param>
        public static void Add<TService, TImplementation>(this IPluginProviderBuilder builder)
            where TService : IPlugin
            where TImplementation : TService =>
            builder.Add(new PluginDescriptor(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="builder"></param>
        public static void Add<TPlugin>(this IPluginProviderBuilder builder)
            where TPlugin : IPlugin =>
            builder.Add(PluginDescriptor.Create<TPlugin>());

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pluginDescriptor"></param>
        private static void TryAdd(IPluginProviderBuilder builder, PluginDescriptor pluginDescriptor)
        {
            var pluginRegistration = builder.PluginDescriptors.FirstOrDefault(x => x.ServiceType == pluginDescriptor.ServiceType);

            if (pluginRegistration is null) {
                builder.Add(pluginDescriptor);
            }
        }

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        public static void TryAdd(this IPluginProviderBuilder builder, Type serviceType, IPlugin plugin) =>
            TryAdd(builder, new PluginDescriptor(serviceType, plugin));

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="builder"></param>
        /// <param name="plugin"></param>
        public static void TryAdd<TService>(this IPluginProviderBuilder builder, IPlugin plugin)
            where TService : IPlugin =>
            TryAdd(builder, new PluginDescriptor(typeof(TService), plugin));

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void TryAdd<TService, TImplementation>(this IPluginProviderBuilder pluginRegistry)
            where TService : IPlugin
            where TImplementation : TService =>
            TryAdd(pluginRegistry, new PluginDescriptor(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void TryAdd<TPlugin>(this IPluginProviderBuilder pluginRegistry)
            where TPlugin : IPlugin =>
            TryAdd(pluginRegistry, PluginDescriptor.Create<TPlugin>());
    }
}
