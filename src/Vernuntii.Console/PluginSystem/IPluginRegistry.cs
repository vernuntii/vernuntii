namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents the read-only interface for accessing plugins.
    /// </summary>
    public interface IPluginRegistry
    {
        /// <summary>
        /// Gets the plugin registration sorted by order.
        /// </summary>
        IEnumerable<IPluginRegistration> OrderlyPluginRegistrations { get; }

        /// <summary>
        /// Gets the plugin by its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        T GetPlugin<T>() where T : IPlugin;
    }
}
