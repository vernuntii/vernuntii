using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins.Events
{
    /// <summary>
    /// Events for <see cref="ICommandLinePlugin"/>.
    /// </summary>
    public static class CommandLineEvents
    {
        /// <summary>
        /// The event is called before a run is started.
        /// </summary>
        public static readonly EventDiscriminator<LifecycleContext> OnBeforeEveryRun = EventDiscriminator.New<LifecycleContext>();

        /// <summary>
        /// Represents an event for dispatching the command line args.
        /// </summary>
        public static readonly EventDiscriminator<string[]> SetCommandLineArguments = EventDiscriminator.New<string[]>();

        /// <summary>
        /// Event when the root command is about to be sealed. Called inside <see cref="ParseCommandLineArguments"/> event.
        /// </summary>
        /// <remarks>
        /// This event is the last opportunity to register arguments, commands and options, or to set the command handler.
        /// </remarks>
        public static readonly EventDiscriminator<IExtensibleCommand> OnSealRootCommand = EventDiscriminator.New<IExtensibleCommand>();

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

        /// <summary>
        /// Represents the lifecycle context for <see cref="ICommandLinePlugin"/>.
        /// </summary>
        public class LifecycleContext
        {
            /// <summary>
            /// The exit code the Vernuntii runner will use.
            /// </summary>
            public int? ExitCode { get; set; }

            /// <summary>
            /// If <see langword="true"/> and an exception has been thrown during command invocation, then exception will be rethrown and the (bad) exit code won't not returned.
            /// </summary>
            public bool PreferExceptionOverExitCode { get; set; }
        }
    }
}
