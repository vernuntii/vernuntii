using System.CommandLine;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// This plugin implements logging capabilities.
    /// </summary>
    public class LoggingPlugin : Plugin, ILoggingPlugin
    {
        /// <inheritdoc/>
        public override int? Order => -2000;

        private Serilog.Core.Logger _logger = null!;
        private ILoggerFactory _loggerFactory = null!;
        private Action<ILoggingBuilder> _loggerBinder = null!;

        /* If option is not specified, then do not log.
         * If value is not specified, then log on information level.
         * If value is specified, then log on specified log level.
         */
        private Option<LogEventLevel?> VerboseOption = new Option<LogEventLevel?>(new[] { "--verbose", "-v" }, parseArgument: result => {
            if (result.Tokens.Count == 0) {
                return LogEventLevel.Information;
            }

            try {
                var argument = new Argument<LogEventLevel>();
                var value = argument.Parse(result.Tokens[0].Value).GetValueForArgument(argument);

                if (!Enum.IsDefined(value)) {
                    result.ErrorMessage = $"Verbosity has not been recognized. Have you accidentally specified a comma-separated value?";
                    return default;
                }

                return value;
            } catch (Exception ex) {
                result.ErrorMessage = ex.Message;
                return default;
            }
        }) {
            Description = "The verbosity level of this application.",
            Arity = ArgumentArity.ZeroOrOne
        };

        private LogEventLevel? _verbosity;

        /// <inheritdoc/>
        protected override void OnCompletedRegistration() =>
            Plugins.First<ICommandLinePlugin>().Registered += plugin => plugin.RootCommand.Add(VerboseOption);

        private void EnableLoggingInfrastructure()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Is(_verbosity ?? LogEventLevel.Fatal)
                .WriteTo.Console(
                    outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    // We want to ouput log to STDERROR
                    standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();

            _loggerBinder = builder => builder.AddSerilog(_logger);
            _loggerFactory = LoggerFactory.Create(builder => _loggerBinder(builder));
            Events.Publish(LoggingEvents.EnabledLoggingInfrastructure, this);
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, parseResult =>
                _verbosity = parseResult.GetValueForOption(VerboseOption));

            Events.SubscribeOnce(LoggingEvents.EnableLoggingInfrastructure, EnableLoggingInfrastructure);
        }

        /// <inheritdoc/>
        public ILogger<T> CreateLogger<T>() =>
            _loggerFactory.CreateLogger<T>();

        /// <inheritdoc/>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string category) =>
            _loggerFactory.CreateLogger(category);

        /// <inheritdoc/>
        public void Bind(ILoggingBuilder builder) => _loggerBinder(builder);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) {
                return;
            }

            _logger.Dispose();
        }
    }
}
