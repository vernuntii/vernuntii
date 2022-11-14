using Microsoft.Extensions.Configuration;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The plugin provides a global <see cref="IConfiguration"/> instance.
    /// </summary>
    public interface IConfigurationPlugin : IPlugin
    {
        /// <summary>
        /// The global <see cref="IConfiguration"/> instance.
        /// </summary>
        IConfiguration Configuration { get; }

        /// <summary>
        /// The configuration file path. Is set after
        /// <see cref="SharedOptionsEvents.ParsedCommandLineArgs"/>.
        /// <see langword="null"/> if no config file could be estimated.
        /// </summary>
        string? ConfigFile { get; }
    }
}
