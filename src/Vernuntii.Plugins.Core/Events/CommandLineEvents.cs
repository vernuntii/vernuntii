using System.CommandLine.Parsing;

namespace Vernuntii.PluginSystem.Events
{
    /// <summary>
    /// Events for <see cref="ICommandLinePlugin"/>.
    /// </summary>
    public static class CommandLineEvents
    {
        /// <summary>
        /// Represents an event for dispatching the command line args.
        /// </summary>
        public readonly static SubjectEvent<string[]> SetCommandLineArgs = new SubjectEvent<string[]>();

        /// <summary>
        /// Event when command line args are getting parsed.
        /// </summary>
        public readonly static SubjectEvent ParseCommandLineArgs = new SubjectEvent();

        /// <summary>
        /// Represents an event for dispatching <see cref="ParseResult"/>.
        /// </summary>
        public readonly static SubjectEvent<ParseResult> ParsedCommandLineArgs = new SubjectEvent<ParseResult>();

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public readonly static SubjectEvent InvokeRootCommand = new SubjectEvent();

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public readonly static SubjectEvent<int> InvokedRootCommand = new SubjectEvent<int>();
    }
}
