namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents a plugin registry.
    /// </summary>
    public interface IPluginRegistry : IPluginRegistrationProducer, IReadOnlyPlugins
    {
        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        ValueTask<IPluginRegistration> RegisterAsync(Type serviceType, IPlugin plugin);
    }
}
