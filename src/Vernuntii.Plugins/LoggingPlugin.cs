using System.CommandLine;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Expressions;
using Serilog.Templates;
using Serilog.Templates.Themes;
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

        private Logger _logger = null!;
        private ILoggerFactory _loggerFactory = null!;
        private Action<ILoggingBuilder> _loggerBinder = null!;

        /* If option is not specified, then do not log.
         * If value is not specified, then log on information level.
         * If value is specified, then log on specified log level.
         */
        private Option<LogEventLevel?> verbosityOption = new Option<LogEventLevel?>(new[] { "--verbosity", "-v" }, parseArgument: result => {
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
        protected override ValueTask OnRegistrationAsync(RegistrationContext registrationContext)
        {
            SerilogPastTime.InitializeLastMoment();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override void OnCompletedRegistration() =>
            Plugins.First<ICommandLinePlugin>().Registered += plugin => plugin.RootCommand.Add(verbosityOption);

        private void EnableLoggingInfrastructure()
        {
            var pastTimeResolver = new StaticMemberNameResolver(typeof(SerilogPastTime));
            var verbosity = _verbosity ?? LogEventLevel.Fatal;

            var expressionTemplate = verbosity == LogEventLevel.Verbose
                ? "[{@l:u3}+{PastTime()}] {@m}\n{@x}"
                : "[{@l:u3}] {@m}\n{@x}";

            _logger = new LoggerConfiguration()
                .MinimumLevel.Is(verbosity)
                // We want to ouput log to STDERROR
                .WriteTo.Console(
                    // "[{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    formatter: new ExpressionTemplate(expressionTemplate,
                        nameResolver: pastTimeResolver,
                        theme: TemplateTheme.Code),
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
                _verbosity = parseResult.GetValueForOption(verbosityOption));

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

        private static class SerilogPastTime
        {
            private static DateTime? _lastMoment;

            public static LogEventPropertyValue? PastTime()
            {
                if (!_lastMoment.HasValue) {
                    throw new InvalidOperationException("You must specify the last moment");
                }

                var now = DateTime.UtcNow;
                var diff = DateTime.UtcNow - _lastMoment.Value;
                _lastMoment = now;
                return new ScalarValue($"{Math.Floor(diff.TotalSeconds)}.{diff.ToString("ff", CultureInfo.InvariantCulture)}");
            }

            public static void InitializeLastMoment()
            {
                if (!_lastMoment.HasValue) {
                    _lastMoment = DateTime.UtcNow;
                }
            }
        }
    }
}
