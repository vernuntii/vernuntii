using Microsoft.Extensions.Configuration;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="IConfigurationPlugin"/>.
    /// </summary>
    public sealed class SharedOptionsEvents
    {
        /// <summary>
        /// Event is happening when the coomand-line args are parsed.
        /// </summary>
        public readonly static SubjectEvent ParseCommandLineArgs = new SubjectEvent();

        /// <summary>
        /// Event is happening when the coomand-line args are parsed.
        /// </summary>
        public readonly static SubjectEvent ParsedCommandLineArgs = new SubjectEvent();
    }
}
