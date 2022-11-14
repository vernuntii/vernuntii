using Vernuntii.Caching;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Plugin that produces the next version and writes it to console.
    /// </summary>
    public interface INextVersionPlugin : IPlugin
    {
        /// <summary>
        /// The exit code used instead when the command-line handler is about to succeed.
        /// </summary>
        int? ExitCodeOnSuccess { get; set; }
    }
}
