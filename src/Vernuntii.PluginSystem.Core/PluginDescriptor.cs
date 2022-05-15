namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Describes a plugin.
    /// </summary>
    public sealed class PluginDescriptor
    {
        /// <summary>
        /// Create a plugin descriptor.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="plugin"></param>
        public static PluginDescriptor Create<TService>(TService plugin)
            where TService : IPlugin =>
            new PluginDescriptor(typeof(TService), plugin);

        /// <summary>
        /// Create a plugin descriptor.
        /// </summary>
        public static PluginDescriptor Create<TService, TImplementation>()
            where TService : IPlugin
            where TImplementation : TService, new() =>
            new PluginDescriptor(typeof(TService), new TImplementation());

        /// <summary>
        /// Create a plugin descriptor.
        /// </summary>
        public static PluginDescriptor Create<TPlugin>()
            where TPlugin : IPlugin, new() =>
            new PluginDescriptor(typeof(IPlugin), new TPlugin());

        /// <summary>
        /// The type of plugin at registration time.
        /// </summary>
        public Type PluginType { get; }
        /// <summary>
        /// The plugin.
        /// </summary>
        public IPlugin Plugin { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="plugin"></param>
        public PluginDescriptor(Type pluginType, IPlugin plugin)
        {
            PluginType = pluginType;
            Plugin = plugin;
        }
    }
}
