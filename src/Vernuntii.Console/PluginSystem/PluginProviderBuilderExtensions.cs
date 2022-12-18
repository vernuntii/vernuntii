namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Extension methods for <see cref="IPluginProviderBuilder"/>
    /// </summary>
    public static class PluginProviderBuilderExtensions
    {
        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="builder"></param>
        public static void Add<TPlugin>(this IPluginProviderBuilder builder)
            where TPlugin : IPlugin =>
            builder.Add(PluginDescriptor.Create<TPlugin>());

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <param name="builder"></param>
        public static void Add<TPluginService, TPluginImplementation>(this IPluginProviderBuilder builder)
            where TPluginService : IPlugin
            where TPluginImplementation : TPluginService =>
            builder.Add(new PluginDescriptor(typeof(TPluginService), typeof(TPluginImplementation)));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="builder"></param>
        /// <param name="plugin"></param>
        public static void Add<TPlugin>(this IPluginProviderBuilder builder, TPlugin plugin)
            where TPlugin : IPlugin =>
            builder.Add(new PluginDescriptor(typeof(TPlugin), plugin));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TPluginService"></typeparam>
        /// <typeparam name="TPluginImplementation"></typeparam>
        /// <param name="builder"></param>
        /// <param name="implementationFactory"></param>
        public static IPluginProviderBuilder Add<TPluginService, TPluginImplementation>(this IPluginProviderBuilder builder, Func<IServiceProvider, TPluginImplementation> implementationFactory)
            where TPluginService : IPlugin
            where TPluginImplementation : class, TPluginService
        {
            builder.Add(PluginDescriptor.Create(implementationFactory));
            return builder;
        }

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pluginDescriptor"></param>
        public static void TryAdd(IPluginProviderBuilder builder, PluginDescriptor pluginDescriptor)
        {
            var pluginRegistration = builder.PluginDescriptors.FirstOrDefault(x => x.ServiceType == pluginDescriptor.ServiceType);

            if (pluginRegistration is null) {
                builder.Add(pluginDescriptor);
            }
        }

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void TryAdd<TPlugin>(this IPluginProviderBuilder pluginRegistry)
            where TPlugin : IPlugin =>
            TryAdd(pluginRegistry, PluginDescriptor.Create<TPlugin>());

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        public static void TryAdd<TPluginService, TPluginImplementation>(this IPluginProviderBuilder pluginRegistry)
            where TPluginService : IPlugin
            where TPluginImplementation : TPluginService =>
            TryAdd(pluginRegistry, new PluginDescriptor(typeof(TPluginService), typeof(TPluginImplementation)));

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
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="builder"></param>
        /// <param name="plugin"></param>
        public static void TryAdd<TPlugin>(this IPluginProviderBuilder builder, IPlugin plugin)
            where TPlugin : IPlugin =>
            TryAdd(builder, PluginDescriptor.Create(plugin));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TPluginService"></typeparam>
        /// <typeparam name="TPluginImplementation"></typeparam>
        /// <param name="builder"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAdd<TPluginService, TPluginImplementation>(this IPluginProviderBuilder builder, Func<IServiceProvider, TPluginImplementation> implementationFactory)
            where TPluginService : IPlugin
            where TPluginImplementation : class, TPluginService =>
            TryAdd(builder, PluginDescriptor.Create(implementationFactory));
    }
}
