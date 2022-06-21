using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The git plugin.
    /// </summary>
    public interface IGitPlugin : IPlugin
    {
        /// <summary>
        /// The git directory. It is available after
        /// <see cref="ConfigurationEvents.ConfiguredConfigurationBuilder"/>.
        /// </summary>
        string GitDirectory { get; }
    }
}
