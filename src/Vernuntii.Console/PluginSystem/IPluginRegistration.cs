using Vernuntii.PluginSystem;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin registration.
    /// </summary>
    public interface IPluginRegistration
    {
        /// <summary>
        /// Unique plugin id.
        /// </summary>
        int PluginId { get; }

        /// <summary>
        /// The implementation type of <see cref="Plugin"/>.
        /// </summary>
        Type ImplementationType { get; }

        /// <summary>
        /// The plugin type for filter purpose.
        /// Must not be the implementation type
        /// of <see cref="Plugin"/>.
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// The registered plugin.
        /// </summary>
        IPlugin Plugin { get; }

        /// <summary>
        /// Indicates whether the registration was successful.
        /// </summary>
        bool Succeeded { get; }
    }
}
