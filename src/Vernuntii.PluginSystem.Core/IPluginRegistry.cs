namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents a plugin registry.
    /// </summary>
    public interface IPluginRegistry : IReadOnlyPlugins
    {
        /// <summary>
        /// Describes a plugin registration.
        /// </summary>
        /// <param name="pluginDescriptor"></param>
        void DescribePluginRegistration(PluginDescriptor pluginDescriptor);
    }
}
