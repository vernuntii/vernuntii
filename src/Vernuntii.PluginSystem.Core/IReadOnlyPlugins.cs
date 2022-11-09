using Vernuntii.PluginSystem.Lifecycle;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents the read-only interface for accessing plugins.
    /// </summary>
    public interface IReadOnlyPlugins
    {
        /// <summary>
        /// Collection consisting of plugin registrations.
        /// </summary>
        IReadOnlyCollection<IPluginRegistration> PluginRegistrations { get; }

        /// <summary>
        /// Gets the plugin by its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        T GetPlugin<T>() where T : IPlugin;
    }
}
