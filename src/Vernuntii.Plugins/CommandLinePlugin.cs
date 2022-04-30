using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// The commandline plugin
    /// </summary>
    [Plugin<ICommandLinePlugin>]
    public class CommandLinePlugin : Plugin, ICommandLinePlugin
    {
        /// <inheritdoc/>
        public override int? Order => -3000;

        private static CommandLineBuilder CreateCommandLineBuilder(RootCommand rootCommand) =>
               new CommandLineBuilder(rootCommand)
                   // .UseVersionOption() // produces exception
                   .UseHelp()
                   .UseEnvironmentVariableDirective()
                   .UseParseDirective()
                   .UseSuggestDirective()
                   .RegisterWithDotnetSuggest()
                   .UseTypoCorrections()
                   .UseParseErrorReporting()
                   //.UseExceptionHandler() // not needed
                   .CancelOnProcessTermination();

        /// <inheritdoc/>
        public Func<RootCommand, CommandLineBuilder> CommandLineBuilderFactory { get; set; } = CreateCommandLineBuilder;
        /// <inheritdoc/>
        public RootCommand RootCommand { get; } = new RootCommand();

        /// <summary>
        /// The command line args received from event.
        /// </summary>
        protected virtual string[]? CommandLineArgs { get; set; }

        private ParseResult _parseResult = null!;

        /// <inheritdoc/>
        public void SetRootCommandHandler(Func<int> action) =>
            RootCommand.Handler = CommandHandler.Create(action);

        /// <summary>
        /// Called when <see cref="CommandLineEvents.ParseCommandLineArgs"/> is happening.
        /// </summary>
        protected virtual void ParseCommandLineArgs()
        {
            if (_parseResult != null) {
                throw new InvalidOperationException("Command line parse result already computed");
            }

            var parser = CommandLineBuilderFactory(RootCommand)
                .AddMiddleware(
                    (ctx, next) => {
                        var invokeRootCommandHandler = _parseResult is not null;
                        _parseResult = ctx.ParseResult;

                        if (invokeRootCommandHandler) {
                            return next(ctx);
                        } else {
                            return Task.CompletedTask;
                        }
                    },
                    MiddlewareOrder.ErrorReporting)
                .Build();

            var exitCode = parser.Invoke(CommandLineArgs ?? Array.Empty<string>());

            if (_parseResult == null) {
                // This is also the case when the help text is displayed but that's okay.
                EventAggregator.PublishEvent(CommandLineEvents.InvokedRootCommand.Discriminator, exitCode);
            } else {
                EventAggregator.PublishEvent(CommandLineEvents.ParsedCommandLineArgs.Discriminator, _parseResult);
            }
        }

        /// <summary>
        /// Called when <see cref="CommandLineEvents.InvokeRootCommand"/> is happening.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual void InvokeRootCommand()
        {
            if (RootCommand.Handler is null) {
                throw new InvalidOperationException("Root command handler is not set");
            }

            var exitCode = _parseResult.Invoke();
            EventAggregator.PublishEvent(CommandLineEvents.InvokedRootCommand.Discriminator, exitCode);
        }

        /// <inheritdoc/>
        protected override void OnEventAggregation()
        {
            SubscribeEvent(CommandLineEvents.SetCommandLineArgs.Discriminator, args => CommandLineArgs = args);
            SubscribeEvent<CommandLineEvents.ParseCommandLineArgs>(ParseCommandLineArgs);
            SubscribeEvent<CommandLineEvents.InvokeRootCommand>(InvokeRootCommand);
        }
    }
}
