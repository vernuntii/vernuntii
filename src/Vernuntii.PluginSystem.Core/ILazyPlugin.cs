namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A delegate that accepts a plugin.
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    /// <param name="plugin"></param>
    public delegate void PluginRegistered<TPlugin>(TPlugin plugin)
        where TPlugin : IPlugin;

    /// <summary>
    /// A lazy plugin.
    /// </summary>
    /// <typeparam name="TPlugin"></typeparam>
    public interface ILazyPlugin<TPlugin> where TPlugin : IPlugin
    {
        /// <summary>
        /// Event when plugin is registered.
        /// </summary>
        event PluginRegistered<TPlugin>? Registered;

        /// <summary>
        /// The lazy plugin.
        /// </summary>
        public TPlugin Value { get; }

        /// <summary>
        /// Indicates whether it is registered.
        /// </summary>
        public bool IsRegistered { get; }
    }
}
