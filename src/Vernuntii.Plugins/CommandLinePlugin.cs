using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.Plugins
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
                // This middleware is not called when an parser exception occured before.
                // This middleware is not called when --help was used.
                // This middleware gets called TWICE but in the first call it will NEVER
                // call the root command!
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

            if (_parseResult is null) {
                // This is the case when the help text is displayed or a
                // parser exception occured but that's okay because we only
                // want to inform about exit code.
                Events.Publish(CommandLineEvents.InvokedRootCommand, exitCode);
            } else {
                Events.Publish(CommandLineEvents.ParsedCommandLineArgs, _parseResult);
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

            // This calls the middleware again.
            var exitCode = _parseResult.Invoke();
            Events.Publish(CommandLineEvents.InvokedRootCommand, exitCode);
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(CommandLineEvents.SetCommandLineArgs, args => CommandLineArgs = args);
            Events.SubscribeOnce(CommandLineEvents.ParseCommandLineArgs, ParseCommandLineArgs);
            Events.Subscribe(CommandLineEvents.InvokeRootCommand, InvokeRootCommand);
        }
    }
}
