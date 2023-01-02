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
        public ICommand RootCommand { get; }

        /// <inheritdoc/>
        internal bool PreferExceptionOverExitCode { get; set; }

        bool ICommandLinePlugin.PreferExceptionOverExitCode {
            get => PreferExceptionOverExitCode;
            set => PreferExceptionOverExitCode = value;
        }

        private readonly RootCommand _rootCommand;
        private readonly SealableCommand _sealableRootCommand;
        private readonly ILogger _logger;
        private ParseResult _parseResult = null!;
        private ExceptionDispatchInfo? _exception;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public CommandLinePlugin(ILogger<CommandLinePlugin> logger)
        {
            _rootCommand = new RootCommand();
            var rootCommand = new HandlerWrappableCommand(_rootCommand, CreateCommandHandler);
            var rootCommandLock = new object();

            _sealableRootCommand = new SealableCommand(
                rootCommand,
                new ReadOnlyCommand(rootCommand, $"The root command is only permitted to be changed before the {CommandLineEvents.ParseCommandLineArguments} event"),
                rootCommandLock);

            RootCommand = new LockingCommandDecorator(_sealableRootCommand, rootCommandLock);
            _logger = logger;
        }

        /// <summary>
        /// A rethrow is attempted if <see cref="PreferExceptionOverExitCode"/> is <see langword="true"/>.
        /// </summary>
        private void AttemptRethrow()
        {
            if (!PreferExceptionOverExitCode) {
                return;
            }

            _exception?.Throw();
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

        private void AttemptLogCommandLineArguments(string[]? commandLineArguments)
        {
            if (!_logger.IsEnabled(LogLevel.Trace)) {
                return;
            }

            string commandLineArgsToEcho;

            if (commandLineArguments == null || commandLineArguments.Length == 0) {
                commandLineArgsToEcho = " (no arguments)";
            } else {
                commandLineArgsToEcho = Environment.NewLine + string.Join(Environment.NewLine, commandLineArguments.Select((x, i) => $"{i}:{x}"));
            }

            _logger.LogTrace("Parse command-line arguments:{CommandLineArgs}", commandLineArgsToEcho);
        }

        private async Task SealRootCommandAsync()
        {
            await Events.FulfillAsync(CommandLineEvents.SealRootCommand, RootCommand);
            _sealableRootCommand.Seal();
            await Events.FulfillAsync(CommandLineEvents.SealedRootCommand, RootCommand);
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.ParseCommandLineArguments"/> is happening.
        /// </summary>
        protected virtual async Task ParseCommandLineArguments(LifecycleContext lifecycleContext, CommandLineArgumentsParsingContext argumentsParsingContext, string[]? commandLineArguments)
        {
            if (_parseResult != null) {
                throw new InvalidOperationException("Command line parse result was already computed");
            }

            await SealRootCommandAsync().ConfigureAwait(false);
            AttemptLogCommandLineArguments(commandLineArguments);
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

            var nonEmptyCommandLineArguments = commandLineArguments ?? Array.Empty<string>();
            var exitCode = await parser.InvokeAsync(nonEmptyCommandLineArguments).ConfigureAwait(false);

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
                .Subscribe(result => {
                    var ((lifecycleContext, commandLineArguments), argumentsParsingContext) = result;
                    return ParseCommandLineArguments(lifecycleContext, argumentsParsingContext, commandLineArguments);
                });

            Events.Every(CommandLineEvents.InvokeRootCommand).Subscribe(InvokeRootCommand);
        }


        private class HandlerWrappableCommand : ICommand
        {
            public ICommandHandler? Handler => _command.Handler;

            private readonly Command _command;
            private readonly Func<Func<Task<int>>?, ICommandHandler?> _commandHandlerFactory;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="command"></param>
            /// <param name="commandHandlerFactory"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public HandlerWrappableCommand(Command command, Func<Func<Task<int>>?, ICommandHandler?> commandHandlerFactory)
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));
                _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            public ICommandHandler? SetHandler(Func<Task<int>>? commandHandler)
            {
                var handler = _commandHandlerFactory(commandHandler);
                _command.Handler = handler;
                return handler;
            }

            public void Add(Argument commandArgument) =>
                _command.Add(commandArgument);

            public void Add(Command command) =>
                _command.Add(command);

            public void Add(Option commandOption) =>
                _command.Add(commandOption);
        }

        private class LockingCommandDecorator : ICommand
        {
            public ICommandHandler? Handler => _command.Handler;

            private readonly ICommand _command;
            private readonly object _commandLock;

            public LockingCommandDecorator(ICommand command, object commandLock)
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));
                _commandLock = commandLock ?? throw new ArgumentNullException(nameof(commandLock));
            }

            public ICommandHandler? SetHandler(Func<Task<int>>? handlerFunc)
            {
                lock (_commandLock) {
                    return _command.SetHandler(handlerFunc);
                }
            }

            public void Add(Argument commandArgument)
            {
                lock (_commandLock) {
                    _command.Add(commandArgument);
                }
            }

            public void Add(Command command)
            {
                lock (_commandLock) {
                    _command.Add(command);
                }
            }

            public void Add(Option commandOption)
            {
                lock (_commandLock) {
                    _command.Add(commandOption);
                }
            }
        }

        private class ReadOnlyCommand : ICommand
        {
            private const string DefaultReadOnlyMessage = "Command is read-only";

            public ICommandHandler? Handler =>
                _command.Handler;

            private ICommand _command;
            private string _readOnlyMessage;

            public ReadOnlyCommand(ICommand handler, string? readOnlyMessage)
            {
                _command = handler ?? throw new ArgumentNullException(nameof(handler));
                _readOnlyMessage = readOnlyMessage ?? DefaultReadOnlyMessage;
            }

            private Exception CreateReadOnlyException() =>
                new InvalidOperationException(_readOnlyMessage);

            public ICommandHandler? SetHandler(Func<Task<int>>? handlerFunc) =>
                throw CreateReadOnlyException();

            public void Add(Argument commandArgument) =>
                throw CreateReadOnlyException();

            public void Add(Command command) =>
                throw CreateReadOnlyException();

            public void Add(Option commandOption) =>
                throw CreateReadOnlyException();
        }

        private class SealableCommand : ICommand
        {
            public ICommandHandler? Handler => _command.Handler;

            private ICommand _command;
            private readonly ICommand _sealedCommand;
            private readonly object _sealLock;

            public SealableCommand(ICommand command, ICommand sealedCommand, object sealLock)
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));
                _sealedCommand = sealedCommand ?? throw new ArgumentNullException(nameof(sealedCommand));
                _sealLock = sealLock ?? throw new ArgumentNullException(nameof(sealLock));
            }

            public ICommandHandler? SetHandler(Func<Task<int>>? handlerFunc) =>
                _command.SetHandler(handlerFunc);

            public void Add(Argument commandArgument) =>
                _command.Add(commandArgument);

            public void Add(Command command) =>
                _command.Add(command);

            public void Add(Option commandOption) =>
                _command.Add(commandOption);

            public void Seal()
            {
                lock (_sealLock) {
                    _command = _sealedCommand;
                }
            }
        }
    }
}
