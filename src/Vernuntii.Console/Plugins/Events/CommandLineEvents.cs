using System.CommandLine.Parsing;
using Vernuntii.CommandLine;
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
        public static readonly EventDiscriminator<string[]> SetCommandLineArguments = EventDiscriminator.New<string[]>();

        /// <summary>
        /// Event when command line args are getting parsed.
        /// </summary>
        public static readonly EventDiscriminator<CommandLineArgumentsParsingContext> ParseCommandLineArguments = EventDiscriminator.New<CommandLineArgumentsParsingContext>();

        /// <summary>
        /// Represents an event for dispatching <see cref="ParseResult"/>.
        /// </summary>
        public static readonly EventDiscriminator<ParseResult> ParsedCommandLineArguments = EventDiscriminator.New<ParseResult>();

        /// <summary>
        /// Event when root command is about to be invoked.
        /// </summary>
        public static readonly EventDiscriminator InvokeRootCommand = EventDiscriminator.New();

        /// <summary>
        /// Event when root command is about to be invoked.
        /// It is also fired when the parsing of arguments failed.
        /// </summary>
        public static readonly EventDiscriminator<int> InvokedRootCommand = EventDiscriminator.New<int>();
    }
}
