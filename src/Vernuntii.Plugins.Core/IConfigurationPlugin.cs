using Microsoft.Extensions.Configuration;

namespace Vernuntii.PluginSystem
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
    }
}
