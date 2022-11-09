using System.CommandLine;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Filters;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Vernuntii.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// This plugin implements logging capabilities.
    /// </summary>
    [Plugin(Order = -2000)]
    public class LoggingPlugin : Plugin, ILoggingPlugin
    {
        private const Verbosity DefaultVerbosity = Verbosity.Fatal;
        const string ConsoleTargetName = nameof(ConsoleTargetName);

        private event Action<ILoggingPlugin>? _enabledLoggingInfrastructureEvent;

        /// <inheritdoc/>
        public Verbosity Verbosity => _verbosity ?? throw new InvalidOperationException("Verbosity is not yet initialized");

        /// <inheritdoc/>
        public bool WriteToStandardError {
            get => _consoleTarget.StdErr;

            set {
                _consoleTarget.StdErr = value;
                AcceptLoggingConfigurationChanges();
            }
        }

        private LoggingConfiguration _loggingConfiguration;
        private ColoredConsoleTarget _consoleTarget;
        private Target _asyncConsoleTarget;
        private BlockTarget _blockTarget;
        private Logger _logger = null!;
        private ILoggerFactory _loggerFactory = null!;
        private Action<ILoggingBuilder> _loggerBinder = null!;

        /* If option is not specified, then do not log.
         * If value is not specified, then log on information level.
         * If value is specified, then log on specified log level.
         */
        private Option<Verbosity?> verbosityOption = new Option<Verbosity?>(new[] { "--verbosity", "-v" }, parseArgument: result => {
            if (result.Tokens.Count == 0) {
                return Verbosity.Information;
            }

            try {
                var argument = new Argument<Verbosity>();
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

        private Verbosity? _verbosity;

        /// <summary>
        /// Creates an instance of this type and enables the debug logging infrastructure.
        /// </summary>
        public LoggingPlugin()
        {
            _loggingConfiguration = new LoggingConfiguration();

            _consoleTarget = new ColoredConsoleTarget() {
                StdErr = true
            };

            _asyncConsoleTarget = new AsyncTargetWrapper(_consoleTarget);
            _blockTarget = new BlockTarget(_asyncConsoleTarget);

            ConfigureLoggingInfrastructure();
        }

        private void SetMinimumVerbosity(Verbosity verbosity)
        {
            if (_loggingConfiguration.LoggingRules.Count != 0) {
                _loggingConfiguration.LoggingRules.RemoveAt(0);
            }

            _loggingConfiguration.AddRule(
                NLog.LogLevel.FromOrdinal((int)verbosity),
                NLog.LogLevel.Off,
                ConsoleTargetName);
        }

        private void AcceptLoggingConfigurationChanges() =>
            _loggingConfiguration.LogFactory.ReconfigExistingLoggers();

        private void ConfigureLoggingInfrastructure()
        {
            _loggingConfiguration.AddTarget(ConsoleTargetName, _blockTarget);
            _logger = _loggingConfiguration.LogFactory.GetCurrentClassLogger();
            _loggerBinder = builder => builder.AddNLog(_loggingConfiguration);
            _loggerFactory = LoggerFactory.Create(builder => _loggerBinder(builder));

            // First we allow any messages since
            // the block target has not been yet
            // unblocked. I case of unblock
            // messages since then are filtered
            // by minimum verbosity again.
            SetMinimumVerbosity(Verbosity.Verbose);
            AcceptLoggingConfigurationChanges();
        }

        private void ReconfigureLoggingInfrastructure()
        {
            lock (_blockTarget.LockObject) {
                var verbosity = _verbosity ??= DefaultVerbosity;
                _blockTarget.Unblock(verbosity);
                SetMinimumVerbosity(verbosity);
                AcceptLoggingConfigurationChanges();
            }
        }

        private void EnableLoggingInfrastructure()
        {
            ReconfigureLoggingInfrastructure();

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
        protected override void OnExecution()
        {
            Plugins.GetPlugin<ICommandLinePlugin>().RootCommand.Add(verbosityOption);

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

            _loggingConfiguration.LogFactory.Shutdown();
            _loggerFactory.Dispose();
        }

        private class BlockTarget : WrapperTargetBase
        {
            public object LockObject { get; } = new object();

            private readonly List<AsyncLogEventInfo> _logEvents;
            private bool _bypassLogEvents;

            public BlockTarget(Target releaseTarget)
            {
                _logEvents = new List<AsyncLogEventInfo>();
                WrappedTarget = releaseTarget;
            }

            public void Unblock(Verbosity? minLevel)
            {
                foreach (var logEvent in _logEvents) {
                    if (!minLevel.HasValue || logEvent.LogEvent.Level.Ordinal >= (int)minLevel) {
                        WrappedTarget.WriteAsyncLogEvent(logEvent);
                    }
                }

                _logEvents.Clear();
                _bypassLogEvents = true;
            }

            protected override void FlushAsync(AsyncContinuation asyncContinuation)
            {
                if (_bypassLogEvents) {
                    Unblock(null);
                }

                base.FlushAsync(asyncContinuation);
            }

            protected override void Write(IList<AsyncLogEventInfo> logEvents)
            {
                if (_bypassLogEvents) {
                    WrappedTarget.WriteAsyncLogEvents(logEvents);
                    return;
                }

                lock (LockObject) {
                    if (_bypassLogEvents) {
                        WrappedTarget.WriteAsyncLogEvents(logEvents);
                    } else {
                        _logEvents.AddRange(logEvents);
                    }
                }
            }

            protected override void Write(AsyncLogEventInfo logEvent)
            {
                if (_bypassLogEvents) {
                    WrappedTarget.WriteAsyncLogEvent(logEvent);
                    return;
                }

                lock (LockObject) {
                    if (_bypassLogEvents) {
                        WrappedTarget.WriteAsyncLogEvent(logEvent);
                    } else {
                        _logEvents.Add(logEvent);
                    }
                }
            }
        }
    }
}
