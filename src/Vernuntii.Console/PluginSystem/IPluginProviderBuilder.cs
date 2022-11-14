namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The interface for adding plugins before the provider is going to be built.
    /// </summary>
    public interface IPluginProviderBuilder
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
