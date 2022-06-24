using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Vernuntii.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using LogLevel = Vernuntii.Logging.LogLevel;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// This plugin implements logging capabilities.
    /// </summary>
    public class LoggingPlugin : Plugin, ILoggingPlugin
    {
        private const LogLevel DefaultVerbosity = LogLevel.Fatal;

        private event Action<ILoggingPlugin>? _enabledLoggingInfrastructureEvent;

        /// <inheritdoc/>
        public override int? Order => -2000;

        /// <inheritdoc/>
        public bool WriteToStandardError {
            get => _consoleTarget.StdErr;

            set {
                _consoleTarget.StdErr = value;
                AcceptLoggingConfigurationChanges();
            }
        }

        private LoggingConfiguration _loggingConfiguration = new LoggingConfiguration();

        private ColoredConsoleTarget _consoleTarget = new ColoredConsoleTarget() {
            StdErr = true
        };

        private Logger _logger = null!;
        private ILoggerFactory _loggerFactory = null!;
        private Action<ILoggingBuilder> _loggerBinder = null!;
        private bool _configuredOnce;

        /* If option is not specified, then do not log.
         * If value is not specified, then log on information level.
         * If value is specified, then log on specified log level.
         */
        private Option<LogLevel?> verbosityOption = new Option<LogLevel?>(new[] { "--verbosity", "-v" }, parseArgument: result => {
            if (result.Tokens.Count == 0) {
                return LogLevel.Information;
            }

            try {
                var argument = new Argument<LogLevel>();
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

        private LogLevel? _verbosity;

        /// <summary>
        /// Creates an instance of this type and enables the debug logging infrastructure.
        /// </summary>
        public LoggingPlugin()
        {
            // Either debug or
            EnableDebugLoggingInfrastructure();
            // release logging infrastructure is enabled
            EnableReleaseLoggingInfrastructure();
            // but never both.
        }

        private void AcceptLoggingConfigurationChanges() =>
            _loggingConfiguration.LogFactory.ReconfigExistingLoggers();

        private void ReconfigureLoggingInfrastructure(LogLevel verbosity)
        {
            const string coloredConsoleTargetName = nameof(coloredConsoleTargetName);

            if (!_configuredOnce) {
                var consoleTarget = new AsyncTargetWrapper(_consoleTarget);

                _loggingConfiguration.AddTarget(coloredConsoleTargetName, consoleTarget);
                AddDefaultRule();

                _logger = _loggingConfiguration.LogFactory.GetCurrentClassLogger();
                _loggerBinder = builder => builder.AddNLog(_loggingConfiguration);
                _loggerFactory = LoggerFactory.Create(builder => _loggerBinder(builder));
                _configuredOnce = true;
            } else {
                RemoveDefaultRule();
                AddDefaultRule();
                AcceptLoggingConfigurationChanges();
            }

            void RemoveDefaultRule() =>
                _loggingConfiguration.LoggingRules.RemoveAt(0);

            void AddDefaultRule()
            {
                _loggingConfiguration.AddRule(
                    NLog.LogLevel.FromOrdinal((int)verbosity),
                    NLog.LogLevel.Off,
                    coloredConsoleTargetName);
            }
        }

        [Conditional("DEBUG")]
        private void EnableDebugLoggingInfrastructure()
        {
            var verbosityString = Environment.GetEnvironmentVariable("Verbosity");

            if (Enum.TryParse<LogLevel>(verbosityString, ignoreCase: true, out var verbosity)) {
                ReconfigureLoggingInfrastructure(verbosity);
            } else {
                ReconfigureLoggingInfrastructure(DefaultVerbosity);
            }
        }

        [Conditional("RELEASE")]
        private void EnableReleaseLoggingInfrastructure() =>
            ReconfigureLoggingInfrastructure(DefaultVerbosity);

        /// <inheritdoc/>
        protected override void OnAfterRegistration()
        {
            Plugins.First<ICommandLinePlugin>().RootCommand.Add(verbosityOption);
        }

        private void DestroyCurrentEnabledLoggingInfrastructure() =>
            _loggerFactory?.Dispose();

        private void EnableLoggingInfrastructure()
        {
            ReconfigureLoggingInfrastructure(_verbosity ?? DefaultVerbosity);

            Events.Publish(LoggingEvents.EnabledLoggingInfrastructure, this);

            if (_enabledLoggingInfrastructureEvent != null) {
                _enabledLoggingInfrastructureEvent.Invoke(this);

                foreach (var handler in _enabledLoggingInfrastructureEvent.GetInvocationList()) {
                    _enabledLoggingInfrastructureEvent -= (Action<ILoggingPlugin>)handler;
                }
            }

            _logger.Trace("Enabled logging infrastructure");
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, parseResult =>
                _verbosity = parseResult.GetValueForOption(verbosityOption));

            Events.SubscribeOnce(LoggingEvents.EnableLoggingInfrastructure, EnableLoggingInfrastructure);
            Events.SubscribeOnce(GlobalServicesEvents.ConfigureServices, sp => sp.AddLogging(Bind));
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

            DestroyCurrentEnabledLoggingInfrastructure();
        }
    }
}
