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
        public sealed class SetCommandLineArgs : PubSubEvent<SetCommandLineArgs, string[]>
        {
        }

        /// <summary>
        /// Event when command line args are getting parsed.
        /// </summary>
        public sealed class ParseCommandLineArgs : PubSubEvent
        {
        }

        /// <summary>
        /// Represents an event for dispatching <see cref="ParseResult"/>.
        /// </summary>
        public sealed class ParsedCommandLineArgs : PubSubEvent<ParsedCommandLineArgs, ParseResult>
        {
        }

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public sealed class InvokeRootCommand : PubSubEvent
        {
        }

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public sealed class InvokedRootCommand : PubSubEvent<InvokedRootCommand, int>
        {
        }
    }
}
