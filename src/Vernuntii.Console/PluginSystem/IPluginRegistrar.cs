namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// An interface for adding plugins.
    /// </summary>
    public interface IPluginRegistrar
    {
        /// <summary>
        /// The plugin descriptors.
        /// </summary>
        IEnumerable<PluginDescriptor> PluginDescriptors { get; }

        /// <summary>
        /// Adds a plugin.
        /// </summary>
        /// <param name="pluginDescriptor"></param>
        void Add(PluginDescriptor pluginDescriptor);
    }
}
