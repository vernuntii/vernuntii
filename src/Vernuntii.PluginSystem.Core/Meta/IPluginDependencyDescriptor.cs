namespace Vernuntii.PluginSystem.Meta
{
    /// <summary>
    /// Describes a dependency of a plugin.
    /// </summary>
    public interface IPluginDependencyDescriptor
    {
        /// <summary>
        /// The type of plugin dependency.
        /// </summary>
        Type PluginDependency { get; }

        /// <summary>
        /// Boolean to indicate whether or not to register the plugin if it is not already registered.
        /// </summary>
        bool TryRegister { get; }
    }
}
