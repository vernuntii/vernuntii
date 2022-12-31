using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Logging;
using Vernuntii.Runner;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.Plugins.Events;
using Vernuntii.Plugins.Lifecycle;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// The commandline plugin.
    /// </summary>
    [Plugin(Order = -3000)]
    public class CommandLinePlugin : Plugin, ICommandLinePlugin
    {
        /// <inheritdoc/>
        private static ICommandHandler? CreateCommandHandler(Func<Task<int>>? action) =>
            action is null ? null : CommandHandler.Create(action);

        /// <inheritdoc/>
        public ICommandWrapper RootCommand { get; }

        /// <inheritdoc/>
        internal bool PreferExceptionOverExitCode { get; set; }

        bool ICommandLinePlugin.PreferExceptionOverExitCode {
            get => PreferExceptionOverExitCode;
            set => PreferExceptionOverExitCode = value;
        }

        private readonly RootCommand _rootCommand;
        private readonly ILogger _logger;
        private ParseResult _parseResult = null!;
        private ExceptionDispatchInfo? _exception;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public CommandLinePlugin(ILogger<CommandLinePlugin> logger)
        {
            _rootCommand = new RootCommand();
            RootCommand = new CommandWrapper(_rootCommand, CreateCommandHandler);
            _logger = logger;
        }

        private void AttemptRethrow()
        {
            if (PreferExceptionOverExitCode) {
                _exception?.Throw();
            }
        }

        private void ConfigureCommandLineBuilder(CommandLineBuilder builder) =>
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
                            await next(ctx).ConfigureAwait(false);
                            _exception = null;
                        } catch (Exception error) {
                            // Unwraps error to make the actual error more valuable for casual users.
                            [Conditional("RELEASE")]
                            static void UnwrapError(ref Exception error)
                            {
                                UnwrapError<TargetInvocationException>(ref error);

                                static void UnwrapError<T>(ref Exception error) where T : Exception
                                {
                                    if (error is T dependencyResolutionException
                                        && dependencyResolutionException.InnerException is not null) {
                                        error = dependencyResolutionException.InnerException;
                                    }
                                }
                            }

                            UnwrapError(ref error);
                            _logger.LogCritical(error, $"{nameof(Vernuntii)} stopped due to an exception.");
                            ctx.ExitCode = (int)ExitCode.Failure;
                            _exception = ExceptionDispatchInfo.Capture(error);
                        }
                    },
                    MiddlewareOrder.ExceptionHandler)
                   .CancelOnProcessTermination();

        /// <summary>
        /// Called when <see cref="CommandLineEvents.ParseCommandLineArguments"/> is happening.
        /// </summary>
        protected virtual async ValueTask ParseCommandLineArguments(LifecycleContext lifecycleContext, CommandLineArgumentsParsingContext argumentsParsingContext, string[]? commandLineArguments)
        {
            if (_parseResult != null) {
                throw new InvalidOperationException("Command line parse result already computed");
            }

            if (_logger.IsEnabled(LogLevel.Trace)) {
                string commandLineArgsToEcho;

                if (commandLineArguments == null || commandLineArguments.Length == 0) {
                    commandLineArgsToEcho = " (no arguments)";
                } else {
                    commandLineArgsToEcho = Environment.NewLine + string.Join(Environment.NewLine, commandLineArguments.Select((x, i) => $"{i}:{x}"));
                }

                _logger.LogTrace("Parse command-line arguments:{CommandLineArgs}", commandLineArgsToEcho);
            }

            var builder = new CommandLineBuilder(_rootCommand);
            ConfigureCommandLineBuilder(builder);

            var parser = builder
                // This middleware is not called when a parser exception occured before.
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

            var exitCode = await parser.InvokeAsync(commandLineArguments ?? Array.Empty<string>()).ConfigureAwait(false);

            // Is null if above middleware was not called.
            if (_parseResult is null) {
                // Short-cirucit; this is the case when the help text is displayed or a
                // parser exception occured but that's okay because we only
                // want to inform about exit code.
                lifecycleContext.ExitCode = exitCode;
            } else {
                argumentsParsingContext.ParseResult = _parseResult;
            }

            AttemptRethrow();
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.InvokeRootCommand"/> is happening.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual async Task InvokeRootCommand()
        {
            if (RootCommand.Handler is null) {
                throw new InvalidOperationException("Root command handler is not set");
            }

            // This calls the middleware again.
            var exitCode = await _parseResult.InvokeAsync().ConfigureAwait(false);
            await Events.FulfillAsync(CommandLineEvents.InvokedRootCommand, exitCode).ConfigureAwait(false);
            AttemptRethrow();
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.Every(LifecycleEvents.BeforeEveryRun)
                .Zip(CommandLineEvents.SetCommandLineArguments)
                .Zip(CommandLineEvents.ParseCommandLineArguments)
                .Subscribe(async result => {
                    var ((lifecycleContext, commandLineArguments), argumentsParsingContext) = result;
                    await ParseCommandLineArguments(lifecycleContext, argumentsParsingContext, commandLineArguments).ConfigureAwait(false);
                });

            Events.Every(CommandLineEvents.InvokeRootCommand).Subscribe(InvokeRootCommand);
        }
    }
}
