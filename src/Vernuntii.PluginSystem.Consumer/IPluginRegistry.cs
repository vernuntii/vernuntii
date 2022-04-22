namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents a plugin registry.
    /// </summary>
    public interface IPluginRegistry : IPluginRegistrationProducer
    {
        /// <summary>
        /// List consisting of plugin registrations.
        /// </summary>
        IReadOnlyList<IPluginRegistration> PluginRegistrations { get; }

        /// <summary>
        /// Registers a plugin.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="plugin"></param>
        IPluginRegistration Register(Type serviceType, IPlugin plugin);

        /// <summary>
        /// The registered plugin.
        /// </summary>
        ILazyPlugin<T> First<T>() where T : IPlugin;
    }
}
