using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Vernuntii.Console;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The commandline plugin.
    /// </summary>
    [Plugin(Order = -3000)]
    public class CommandLinePlugin : Plugin, ICommandLinePlugin
    {
        private static void ConfigureCommandLineBuilder(CommandLineBuilder builder, ILogger logger) =>
               builder
                   // .UseVersionOption() // produces exception
                   .UseHelp()
                   .UseEnvironmentVariableDirective()
                   .UseParseDirective()
                   .UseSuggestDirective()
                   .RegisterWithDotnetSuggest()
                   .UseTypoCorrections()
                   .UseParseErrorReporting()
                   .AddMiddleware(
                    async (ctx, next) => {
                        try {
                            await next(ctx);
                        } catch (Exception error) {
                            UnwrapError(ref error);
                            logger.LogCritical(error, $"{nameof(Vernuntii)} stopped due to an exception.");
                            ctx.ExitCode = (int)ExitCode.Failure;

                            // Unwraps error to make the actual error more valuable for casual users.
                            [Conditional("RELEASE")]
                            static void UnwrapError(ref Exception error)
                            {
                                UnwrapError<TargetInvocationException>(ref error);
                                //UnwrapError<DependencyResolutionException>(ref error);

                                static void UnwrapError<T>(ref Exception error) where T : Exception
                                {
                                    if (error is T dependencyResolutionException
                                        && dependencyResolutionException.InnerException is not null) {
                                        error = dependencyResolutionException.InnerException;
                                    }
                                }
                            }
                        }
                    },
                    MiddlewareOrder.ExceptionHandler)
                   //.UseExceptionHandler() // not needed
                   .CancelOnProcessTermination();

        /// <inheritdoc/>
        private static ICommandHandler? CreateCommandHandler(Func<int>? action) =>
            action is null ? null : CommandHandler.Create(action);

        /// <inheritdoc/>
        public ICommandWrapper RootCommand { get; }

        /// <inheritdoc/>
        public Action<CommandLineBuilder, ILogger> ConfigureCommandLineBuilderAction { get; set; } = ConfigureCommandLineBuilder;

        /// <summary>
        /// The command line args received from event.
        /// </summary>
        protected virtual string[]? CommandLineArgs { get; set; }

        private readonly RootCommand _rootCommand;
        private readonly ILogger _logger;
        private ParseResult _parseResult = null!;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public CommandLinePlugin(ILogger<CommandLinePlugin> logger)
        {
            _rootCommand = new RootCommand();
            RootCommand = new CommandWrapper(_rootCommand, CreateCommandHandler);
            _logger = logger;
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.ParseCommandLineArgs"/> is happening.
        /// </summary>
        protected virtual void ParseCommandLineArgs()
        {
            if (_parseResult != null) {
                throw new InvalidOperationException("Command line parse result already computed");
            }

            if (_logger.IsEnabled(LogLevel.Trace)) {
                string commandLineArgsToEcho;

                if (CommandLineArgs == null || CommandLineArgs.Length == 0) {
                    commandLineArgsToEcho = " (no arguments)";
                } else {
                    commandLineArgsToEcho = Environment.NewLine + string.Join(Environment.NewLine, CommandLineArgs.Select((x, i) => $"{i}:{x}"));
                }

                _logger.LogTrace("Parse command-line arguments:{CommandLineArgs}", commandLineArgsToEcho);
            }

            var builder = new CommandLineBuilder(_rootCommand);
            ConfigureCommandLineBuilderAction(builder, _logger);

            var parser = builder
                // This middleware is not called when an parser exception occured before.
                // This middleware is not called when --help was used.
                // This middleware gets called TWICE but in the first call it will NEVER call the root command!
                .AddMiddleware(
                    (ctx, next) => {
                        var firstCall = _parseResult is null;
                        _parseResult = ctx.ParseResult;

                        if (firstCall) {
                            return Task.CompletedTask;
                        } else {
                            return next(ctx);
                        }
                    },
                    MiddlewareOrder.ErrorReporting)
                .Build();

            var exitCode = parser.Invoke(CommandLineArgs ?? Array.Empty<string>());

            // Is null if above middleware was not called.
            if (_parseResult is null) {
                // This is the case when the help text is displayed or a
                // parser exception occured but that's okay because we only
                // want to inform about exit code.
                Events.FireEvent(CommandLineEvents.InvokedRootCommand, exitCode);
            } else {
                Events.FireEvent(CommandLineEvents.ParsedCommandLineArgs, _parseResult);
            }
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.InvokeRootCommand"/> is happening.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void InvokeRootCommand()
        {
            if (RootCommand.HandlerFunc is null) {
                throw new InvalidOperationException("Root command handler is not set");
            }

            // This calls the middleware again.
            var exitCode = _parseResult.Invoke();
            Events.FireEvent(CommandLineEvents.InvokedRootCommand, exitCode);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.OnNextEvent(CommandLineEvents.SetCommandLineArgs, args => CommandLineArgs = args);
            Events.OnNextEvent(CommandLineEvents.ParseCommandLineArgs, ParseCommandLineArgs);
            Events.OnEveryEvent(CommandLineEvents.InvokeRootCommand, InvokeRootCommand);
        }
    }
}
