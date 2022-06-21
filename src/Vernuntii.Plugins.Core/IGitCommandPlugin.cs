using Vernuntii.Git.Command;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The git command plugin. It provides a pre-initlaized git command.
    /// </summary>
    public interface IGitCommandPlugin : IPlugin
    {
        /// <summary>
        /// The git command.
        /// </summary>
        IGitCommand GitCommand { get; }
    }
}
