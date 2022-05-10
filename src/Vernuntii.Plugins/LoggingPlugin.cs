using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
        private event Action<ILoggingPlugin>? _enabledLoggingInfrastructureEvent;

        /// <inheritdoc/>
        public override int? Order => -2000;

        private Logger _logger = null!;
        private ILoggerFactory? _loggerFactory;
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

        /// <summary>
        /// Creates an instance of this type and enables the debug logging infrastructure.
        /// </summary>
        public LoggingPlugin() =>
            EnableDebugLoggingInfrastructure();

        [Conditional("DEBUG")]
        private void EnableDebugLoggingInfrastructure() =>
            EnableLoggingInfrastructure(publishEvents: false, overrideVerbosity: LogEventLevel.Verbose);

        /// <inheritdoc/>
        protected override void OnCompletedRegistration()
        {
            SerilogPastTime.InitializeLastMoment();
            Plugins.First<ICommandLinePlugin>().Registered += plugin => plugin.RootCommand.Add(verbosityOption);
        }

        private void DestroyCurrentEnableLogginInfrastructure()
        {
            _loggerFactory?.Dispose();
            _logger?.Dispose();
        }

        private void EnableLoggingInfrastructure(bool publishEvents, LogEventLevel? overrideVerbosity = null)
        {
            DestroyCurrentEnableLogginInfrastructure();

            var pastTimeResolver = new StaticMemberNameResolver(typeof(SerilogPastTime));
            var verbosity = overrideVerbosity ?? _verbosity ?? LogEventLevel.Fatal;

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

            if (publishEvents) {
                Events.Publish(LoggingEvents.EnabledLoggingInfrastructure, this);

                if (_enabledLoggingInfrastructureEvent != null) {
                    _enabledLoggingInfrastructureEvent.Invoke(this);

                    foreach (var handler in _enabledLoggingInfrastructureEvent.GetInvocationList()) {
                        _enabledLoggingInfrastructureEvent -= (Action<ILoggingPlugin>)handler;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, parseResult =>
                _verbosity = parseResult.GetValueForOption(verbosityOption));

            Events.SubscribeOnce(
                LoggingEvents.EnableLoggingInfrastructure,
                () => EnableLoggingInfrastructure(publishEvents: true));
        }

        /// <inheritdoc/>
        public ILogger<T> CreateLogger<T>() =>
            _loggerFactory?.CreateLogger<T>() ?? new LazyLogger<T>(_enabledLoggingInfrastructureEvent);

        /// <inheritdoc/>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string category) =>
            _loggerFactory?.CreateLogger(category) ?? new LazyLogger(category, _enabledLoggingInfrastructureEvent);

        /// <inheritdoc/>
        public void Bind(ILoggingBuilder builder) => _loggerBinder(builder);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) {
                return;
            }

            DestroyCurrentEnableLogginInfrastructure();
        }

        private static class SerilogPastTime
        {
            private static Stopwatch? _lastMoment;

            public static LogEventPropertyValue? PastTime()
            {
                if (_lastMoment is null) {
                    throw new InvalidOperationException("You must specify the last moment");
                }

                var elapsedTime = _lastMoment.Elapsed;
                _lastMoment.Reset();
                _lastMoment.Start();
                return new ScalarValue($"{Math.Floor(elapsedTime.TotalSeconds)}.{elapsedTime.ToString("ff", CultureInfo.InvariantCulture)}");
            }

            public static void InitializeLastMoment()
            {
                if (_lastMoment is null) {
                    _lastMoment = new Stopwatch();
                    _lastMoment.Start();
                }
            }
        }

        private class LazyLogger : Microsoft.Extensions.Logging.ILogger
        {
            private Microsoft.Extensions.Logging.ILogger _logger = NullLogger.Instance;

            public LazyLogger(string category, Action<ILoggingPlugin>? whenEnabledLoggingInfrastructure) =>
                whenEnabledLoggingInfrastructure += loggingPlugin => _logger = loggingPlugin.CreateLogger(category);

            IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) => _logger.BeginScope(state);

            bool Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

            void Microsoft.Extensions.Logging.ILogger.Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        private class LazyLogger<T> : ILogger<T>
        {
            private ILogger<T> _logger = NullLogger<T>.Instance;

            public LazyLogger(Action<LoggingPlugin>? whenEnabledLoggingInfrastructure) =>
                whenEnabledLoggingInfrastructure += loggingPlugin => _logger = loggingPlugin.CreateLogger<T>();

            IDisposable Microsoft.Extensions.Logging.ILogger.BeginScope<TState>(TState state) => _logger.BeginScope(state);

            bool Microsoft.Extensions.Logging.ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

            void Microsoft.Extensions.Logging.ILogger.Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
