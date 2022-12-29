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
        public static readonly EventDiscriminator ParseCommandLineArguments = new();

        /// <summary>
        /// Event is happening when the coomand-line args are parsed.
        /// </summary>
        public static readonly EventDiscriminator ParsedCommandLineArguments = new();
    }
}
