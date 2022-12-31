using Vernuntii.Plugins;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Extension methods for <see cref="IPluginRegistrar"/>
    /// </summary>
    public static class PluginRegistrarExtensions
    {
        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="plugins"></param>
        public static void Add<TPlugin>(this IPluginRegistrar plugins)
            where TPlugin : IPlugin =>
            plugins.Add(PluginDescriptor.Create<TPlugin>());

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <param name="plugins"></param>
        public static void Add<TPluginService, TPluginImplementation>(this IPluginRegistrar plugins)
            where TPluginService : IPlugin
            where TPluginImplementation : TPluginService =>
            plugins.Add(new PluginDescriptor(typeof(TPluginService), typeof(TPluginImplementation)));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="plugin"></param>
        public static void Add<TPlugin>(this IPluginRegistrar plugins, TPlugin plugin)
            where TPlugin : IPlugin =>
            plugins.Add(new PluginDescriptor(typeof(TPlugin), plugin));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TPluginService"></typeparam>
        /// <typeparam name="TPluginImplementation"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="implementationFactory"></param>
        public static IPluginRegistrar Add<TPluginService, TPluginImplementation>(this IPluginRegistrar plugins, Func<IServiceProvider, TPluginImplementation> implementationFactory)
            where TPluginService : IPlugin
            where TPluginImplementation : class, TPluginService
        {
            plugins.Add(PluginDescriptor.Create(implementationFactory));
            return plugins;
        }

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="plugins"></param>
        /// <param name="pluginDescriptor"></param>
        public static void TryAdd(IPluginRegistrar plugins, PluginDescriptor pluginDescriptor)
        {
            var pluginRegistration = plugins.PluginDescriptors.FirstOrDefault(x => x.ServiceType == pluginDescriptor.ServiceType);

            if (pluginRegistration is null) {
                plugins.Add(pluginDescriptor);
            }
        }

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="plugins"></param>
        public static void TryAdd<TPlugin>(this IPluginRegistrar plugins)
            where TPlugin : IPlugin =>
            TryAdd(plugins, PluginDescriptor.Create<TPlugin>());

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="plugins"></param>
        public static void TryAdd<TPluginService, TPluginImplementation>(this IPluginRegistrar plugins)
            where TPluginService : IPlugin
            where TPluginImplementation : TPluginService =>
            TryAdd(plugins, new PluginDescriptor(typeof(TPluginService), typeof(TPluginImplementation)));

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <param name="plugins"></param>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        public static void TryAdd(this IPluginRegistrar plugins, Type serviceType, IPlugin plugin) =>
            TryAdd(plugins, new PluginDescriptor(serviceType, plugin));

        /// <summary>
        /// Tries to add a plugin.
        /// </summary>
        /// <typeparam name="TPlugin"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="plugin"></param>
        public static void TryAdd<TPlugin>(this IPluginRegistrar plugins, IPlugin plugin)
            where TPlugin : IPlugin =>
            TryAdd(plugins, PluginDescriptor.Create(plugin));

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <typeparam name="TPluginService"></typeparam>
        /// <typeparam name="TPluginImplementation"></typeparam>
        /// <param name="plugins"></param>
        /// <param name="implementationFactory"></param>
        public static void TryAdd<TPluginService, TPluginImplementation>(this IPluginRegistrar plugins, Func<IServiceProvider, TPluginImplementation> implementationFactory)
            where TPluginService : IPlugin
            where TPluginImplementation : class, TPluginService =>
            TryAdd(plugins, PluginDescriptor.Create(implementationFactory));

        /// <summary>
        /// Adds all required plugins to calculate the next version.
        /// </summary>
        /// <param name="plugins"></param>
        public static IPluginRegistrar AddNextVersionRequirements(this IPluginRegistrar plugins)
        {
            plugins.Add<IGitPlugin, GitPlugin>();
            plugins.Add<INextVersionPlugin, NextVersionPlugin>();
            return plugins;
        }
    }
}
