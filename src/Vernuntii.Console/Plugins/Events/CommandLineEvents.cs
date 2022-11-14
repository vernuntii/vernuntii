using System.CommandLine.Parsing;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="ICommandLinePlugin"/>.
    /// </summary>
    public static class CommandLineEvents
    {
        /// <summary>
        /// Represents an event for dispatching the command line args.
        /// </summary>
        public static readonly SubjectEvent<string[]> SetCommandLineArgs = new();

        /// <summary>
        /// Event when command line args are getting parsed.
        /// </summary>
        public static readonly SubjectEvent ParseCommandLineArgs = new();

        /// <summary>
        /// Represents an event for dispatching <see cref="ParseResult"/>.
        /// </summary>
        public static readonly SubjectEvent<ParseResult> ParsedCommandLineArgs = new();

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public static readonly SubjectEvent InvokeRootCommand = new();

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public static readonly SubjectEvent<int> InvokedRootCommand = new();
    }
}
