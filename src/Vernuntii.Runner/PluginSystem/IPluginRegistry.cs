using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Represents the read-only interface for accessing plugins.
    /// </summary>
    public interface IPluginRegistry
    {
        /// <summary>
        /// Gets the plugin registrations.
        /// </summary>
        IEnumerable<IPluginRegistration> PluginRegistrations { get; }

        /// <summary>
        /// Gets the plugin by its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        T GetPlugin<T>() where T : IPlugin;

        /// <summary>
        /// Tries to get the plugin by its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plugin"></param>
        /// <returns>
        /// <see langword="true"/> if the plugin existed.
        /// </returns>
        bool TryGetPlugin<T>([MaybeNullWhen(false)] out T plugin) where T : IPlugin;
    }
}
