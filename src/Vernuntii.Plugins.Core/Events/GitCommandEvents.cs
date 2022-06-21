using Microsoft.Extensions.Configuration;
using Vernuntii.Git.Command;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IGitCommandPlugin"/>.
    /// </summary>
    public sealed class GitCommandEvents
    {
        /// <summary>
        /// Event is happening when the git command has been created.
        /// </summary>
        public readonly static SubjectEvent<IGitCommand> CreatedCommand = new SubjectEvent<IGitCommand>();
    }
}
