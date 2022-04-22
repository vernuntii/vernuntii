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
        public sealed class SetCommandLineArgsEvent : PubSubEvent<SetCommandLineArgsEvent, string[]>
        {
        }

        /// <summary>
        /// Event when command line args are getting parsed.
        /// </summary>
        public sealed class ParseCommandLineArgsEvent : PubSubEvent
        {
        }

        /// <summary>
        /// Represents an event for dispatching <see cref="ParseResult"/>.
        /// </summary>
        public sealed class ParsedCommandLineArgsEvent : PubSubEvent<ParsedCommandLineArgsEvent, ParseResult>
        {
        }

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public sealed class InvokeRootCommandEvent : PubSubEvent
        {
        }

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public sealed class InvokedRootCommandEvent : PubSubEvent<InvokedRootCommandEvent, int>
        {
        }
    }
}
