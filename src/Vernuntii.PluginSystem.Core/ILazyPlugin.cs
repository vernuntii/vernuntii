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
        /// Event when plugin is registered. When adding delegate after
        /// registration phase the delegate gets fired immediatelly.
        /// After registration phase the delegates won't be cached
        /// and existing delegates are cleared.
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
