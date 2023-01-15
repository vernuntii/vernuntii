using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.CommandLine.NamingConventionBinder;
using Microsoft.Extensions.Logging;
using Vernuntii.Runner;
using Vernuntii.Plugins.CommandLine;
using Vernuntii.Plugins.Events;
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
        private static ICommandHandler? WrapCommandHandler(Func<Task<int>>? action) =>
            action is null ? null : CommandHandler.Create(action); // CommandHandler originates from NamingConventionBinder assembly

        /// <inheritdoc/>
        public IExtensibleCommand RootCommand => _rootCommandWrapper;

        private readonly RootCommand _rootCommand;
        private readonly ICommand _rootCommandWrapper;
        private readonly object _rootCommandLock;
        private readonly SwappableCommand _sealableRootCommand;
        private readonly ILogger _logger;
        private ParseResult _parseResult = null!;
        private bool _allowCommandHandlerInvocation;
        private ExceptionDispatchInfo? _exception;
        private CommandSeat? _lastRequestedCommandSeat;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public CommandLinePlugin(ILogger<CommandLinePlugin> logger)
        {
            _rootCommand = new RootCommand();
            var rootCommand = new HandlerWrappingCommand(_rootCommand, WrapCommandHandler);

            _sealableRootCommand = new SwappableCommand(
                rootCommand,
                new ReadOnlyCommand($"The root command is only permitted to be changed before the {CommandLineEvents.ParseCommandLineArguments}/{CommandLineEvents.OnSealRootCommand} event"));

            _rootCommandLock = new object();
            _rootCommandWrapper = new LockingCommandDecorator(_sealableRootCommand, _rootCommandLock);
            _logger = logger;
        }

        /// <inheritdoc cref="ICommandLinePlugin.RequestRootCommandSeat"/>
        public ICommandSeat RequestRootCommandSeat()
        {
            lock (_rootCommandLock) {
                return _lastRequestedCommandSeat = new CommandSeat();
            }
        }

        private Task OnBeforeEveryRun()
        {
            _exception = null;
            return Events.EmitAsync(CommandLineEvents.OnBeforeEveryRun, new CommandLineEvents.LifecycleContext());
        }

        /// <summary>
        /// A rethrow is attempted if <see cref="CommandLineEvents.LifecycleContext.PreferExceptionOverExitCode"/> is <see langword="true"/>.
        /// </summary>
        private void AttemptRethrow()
        {
            _exception?.Throw();

            // Maybe some parsing errors?
            if (_parseResult.Errors.Count > 0) {
                throw new CommandLineArgumentsException($"""
                    While parsing the command-line arguments, one or more parsing errors occured:
                    - {string.Join(Environment.NewLine + "- ", _parseResult.Errors.Select(x => x.Message))}
                    """);
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
                   // This middleware is called as many times as ParseResult.InvokeAsync is invoked.
                   // This middleware is called when a parser exception occured before.
                   // This middleware is called when --help was used.
                   .AddMiddleware(
                    async (context, next) => {
                        try {
                            // Neither does a middleware throw if the parsing fails or the help text is displayed, nor does the pipeline continue from that middleware.
                            // "ParseCommandLineArguments" relies on the latter. In case of failing parsing, a bad exit code is returned on "ParseResult.InvokeAsync".
                            await next(context).ConfigureAwait(false);
                            _exception = null;
                        } catch (Exception error) {
                            // Unwraps error to make the actual error more valuable for casual users.
                            [Conditional("RELEASE")]
                            static void UnwrapError<T>(ref Exception error) where T : Exception
                            {
                                if (error is T dependencyResolutionException
                                    && dependencyResolutionException.InnerException is not null) {
                                    error = dependencyResolutionException.InnerException;
                                }
                            }

                            UnwrapError<TargetInvocationException>(ref error);
                            _logger.LogCritical(error, $"{nameof(Vernuntii)} stopped due to an exception.");
                            context.ExitCode = (int)ExitCode.Failure;
                            _exception = ExceptionDispatchInfo.Capture(error);
                        }

                        // After the parse result could have been changed the last, we capture it to operate on it later on
                        _parseResult = context.ParseResult;
                    },
                    MiddlewareOrder.ExceptionHandler)
                   .CancelOnProcessTermination();

        private void AttemptLoggingCommandLineArguments(string[]? commandLineArguments)
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
            await Events.EmitAsync(CommandLineEvents.OnSealRootCommand, RootCommand);

            lock (_rootCommandLock) {
                _lastRequestedCommandSeat?.TakeSeat(_rootCommandWrapper);
                _sealableRootCommand.Swap(swapped: true);
            }
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.ParseCommandLineArguments"/> is happening.
        /// </summary>
        protected virtual async Task ParseCommandLineArguments(
            CommandLineEvents.LifecycleContext lifecycleContext,
            CommandLineArgumentsParsingContext argumentsParsingContext,
            string[]? commandLineArguments)
        {
            if (_parseResult != null) {
                throw new InvalidOperationException("Command line parse result was already computed");
            }

            await SealRootCommandAsync().ConfigureAwait(false);
            AttemptLoggingCommandLineArguments(commandLineArguments);
            var builder = new CommandLineBuilder(_rootCommand);
            ConfigureCommandLineBuilder(builder);

            var parser = builder
                // This middleware is not called when a parser exception occured before.
                // This middleware is not called when --help was used.
                // This middleware gets called TWICE, but in the first call we do not invoke the root command, instead we parse!
                .AddMiddleware(
                    (ctx, next) => _allowCommandHandlerInvocation ? next(ctx) : Task.CompletedTask,
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

            if (lifecycleContext.PreferExceptionOverExitCode) {
                AttemptRethrow();
            }
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.InvokeRootCommand"/> is happening.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual async Task InvokeRootCommand(CommandLineEvents.LifecycleContext lifecycleContext)
        {
            if (_rootCommand.Handler is null) {
                throw new InvalidOperationException("The root command handler has not been set");
            }

            _allowCommandHandlerInvocation = true;
            var exitCode = await _parseResult.InvokeAsync().ConfigureAwait(false); // Call the middleware again
            await Events.EmitAsync(CommandLineEvents.InvokedRootCommand, exitCode).ConfigureAwait(false);

            if (lifecycleContext.PreferExceptionOverExitCode) {
                AttemptRethrow();
            }
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events.Every(LifecycleEvents.BeforeEveryRun).Subscribe(OnBeforeEveryRun);

            Events.Once(CommandLineEvents.OnBeforeEveryRun)
                .Zip(CommandLineEvents.SetCommandLineArguments)
                .Zip(CommandLineEvents.ParseCommandLineArguments)
                .Subscribe(result => {
                    var ((lifecycleContext, commandLineArguments), argumentsParsingContext) = result;
                    return ParseCommandLineArguments(lifecycleContext, argumentsParsingContext, commandLineArguments);
                });

            Events.Every(CommandLineEvents.OnBeforeEveryRun)
                .Zip(CommandLineEvents.InvokeRootCommand).Subscribe(result => {
                    var (lifecycleContext, _) = result;
                    return InvokeRootCommand(lifecycleContext);
                });
        }

        private class HandlerWrappingCommand : ICommand
        {
            public bool IsReadOnly => false;

            private readonly Command _command;
            private readonly Func<Func<Task<int>>?, ICommandHandler?> _commandHandlerFactory;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="command"></param>
            /// <param name="commandHandlerFactory"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public HandlerWrappingCommand(Command command, Func<Func<Task<int>>?, ICommandHandler?> commandHandlerFactory)
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));
                _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
            }

            public void SetHandler(Func<Task<int>>? commandHandler) =>
                _command.Handler = _commandHandlerFactory(commandHandler);

            public void Add(Argument commandArgument) =>
                _command.Add(commandArgument);

            public void Add(Command command) =>
                _command.Add(command);

            public void Add(Option commandOption) =>
                _command.Add(commandOption);
        }

        private class LockingCommandDecorator : ICommand
        {
            public bool IsReadOnly => _command.IsReadOnly;

            private readonly ICommand _command;
            private readonly object _commandLock;

            public LockingCommandDecorator(ICommand command, object commandLock)
            {
                _command = command ?? throw new ArgumentNullException(nameof(command));
                _commandLock = commandLock ?? throw new ArgumentNullException(nameof(commandLock));
            }

            public void SetHandler(Func<Task<int>>? handlerFunc)
            {
                lock (_commandLock) {
                    _command.SetHandler(handlerFunc);
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
            public bool IsReadOnly => true;

            private const string DefaultReadOnlyMessage = "Command is read-only and cannot be changed";

            private string _readOnlyMessage;

            public ReadOnlyCommand(string? readOnlyMessage) =>
                _readOnlyMessage = readOnlyMessage ?? DefaultReadOnlyMessage;

            private Exception CreateReadOnlyException() =>
                new InvalidOperationException(_readOnlyMessage);

            public void SetHandler(Func<Task<int>>? handlerFunc) =>
                throw CreateReadOnlyException();

            public void Add(Argument commandArgument) =>
                throw CreateReadOnlyException();

            public void Add(Command command) =>
                throw CreateReadOnlyException();

            public void Add(Option commandOption) =>
                throw CreateReadOnlyException();
        }

        private class SwappableCommand : ICommand
        {
            public bool IsReadOnly => _command.IsReadOnly;

            private ICommand _command;
            private readonly ICommand _originalCommand;
            private readonly ICommand _swappableCommand;

            public SwappableCommand(ICommand command, ICommand swappableCommand)
            {
                _originalCommand = command ?? throw new ArgumentNullException(nameof(command));
                _swappableCommand = swappableCommand ?? throw new ArgumentNullException(nameof(swappableCommand));
                _command = _originalCommand;
            }

            public void SetHandler(Func<Task<int>>? handlerFunc) =>
                _command.SetHandler(handlerFunc);

            public void Add(Argument commandArgument) =>
                _command.Add(commandArgument);

            public void Add(Command command) =>
                _command.Add(command);

            public void Add(Option commandOption) =>
                _command.Add(commandOption);

            public void Swap(bool swapped)
            {
                if (swapped) {
                    _command = _swappableCommand;
                } else {
                    _command = _originalCommand;
                }
            }
        }
    }
}
