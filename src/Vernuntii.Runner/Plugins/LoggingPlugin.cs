using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Layouts;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Vernuntii.CommandLine;
using Vernuntii.Logging;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// This plugin implements logging capabilities.
    /// </summary>
    [Plugin(Order = -2000)]
    public class LoggingPlugin : Plugin, ILoggingPlugin
    {
        private const Verbosity DefaultVerbosity = Verbosity.Fatal;
        private const string ConsoleTargetName = nameof(ConsoleTargetName);

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

        private readonly LoggingConfiguration _loggingConfiguration;
        private readonly ColoredConsoleTarget _consoleTarget;
        private readonly Target _asyncConsoleTarget;
        private readonly BlockTarget _blockTarget;
        private Logger _logger = null!;
        private ILoggerFactory _loggerFactory;
        private Action<ILoggingBuilder> _loggerBinder = null!;

        /* If option is not specified, then do not log.
         * If value is not specified, then log on information level.
         * If value is specified, then log on specified log level.
         */
        private readonly Option<Verbosity?> _verbosityOption = new(new[] { "--verbose", "--verbosity", "-v" }, parseArgument: result => {
            if (result.IsParentOptionTokenEquals("--verbose") && result.Tokens.Count != 0) {
                result.ErrorMessage = "When using --verbose, you cannot specify a verbosity. Use --verbosity instead.";
                return default;
            } else if (result.IsParentOptionTokenEquals("--verbosity") && result.Tokens.Count == 0) {
                result.ErrorMessage = "When using --verbosity, you must specify a verbosity. Use --verbose instead.";
                return default;
            }

            if (result.Tokens.Count == 0) {
                return Verbosity.Information;
            }

            try {
                var argument = new Argument<Verbosity>();
                var value = argument.Parse(result.Tokens[0].Value).GetValueForArgument(argument);

                if (!Enum.IsDefined(value)) {
                    result.ErrorMessage = $"Verbosity has not been recognized. Have you specified a comma-separated value by accident?";
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
        private readonly IPluginRegistry _pluginRegistry;

        /// <summary>
        /// Creates an instance of this type and enables the debug logging infrastructure.
        /// </summary>
        public LoggingPlugin(IPluginRegistry pluginRegistry)
        {
            _loggingConfiguration = new LoggingConfiguration();

            _consoleTarget = new ColoredConsoleTarget() {
                StdErr = true,
                Layout = Layout.FromString("${time}|${level:uppercase=true}|${logger:shortName=true}|${message:withexception=true}")
            };

            _asyncConsoleTarget = new AsyncTargetWrapper(_consoleTarget);
            _blockTarget = new BlockTarget(_asyncConsoleTarget);

            ConfigureLoggingInfrastructure();
            _pluginRegistry = pluginRegistry;
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

        [MemberNotNull(nameof(_loggerFactory))]
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

        private async Task EnableLoggingInfrastructure()
        {
            ReconfigureLoggingInfrastructure();

            await Events.EmitAsync(LoggingEvents.OnEnabledLoggingInfrastructure, this).ConfigureAwait(false);

            if (_enabledLoggingInfrastructureEvent != null) {
                _enabledLoggingInfrastructureEvent.Invoke(this);

                foreach (var handler in _enabledLoggingInfrastructureEvent.GetInvocationList()) {
                    _enabledLoggingInfrastructureEvent -= (Action<ILoggingPlugin>)handler;
                }
            }

            _logger.Trace("Enabled logging infrastructure");
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
        public void AddProvider(ILoggerProvider provider) =>
            _loggerFactory.AddProvider(provider);

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            // We need to lazy load, because command line plugin has dependency on this plugin
            _pluginRegistry.GetPlugin<ICommandLinePlugin>().RootCommand.Add(_verbosityOption);

            Events.Once(CommandLineEvents.ParsedCommandLineArguments)
                .Subscribe(parseResult => _verbosity = parseResult.GetValueForOption(_verbosityOption));

            Events.Once(LoggingEvents.EnableLoggingInfrastructure).Subscribe(EnableLoggingInfrastructure);
            Events.Once(ServicesEvents.OnConfigureServices).Subscribe(sp => sp.AddLogging(Bind));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _loggingConfiguration.LogFactory.Flush();
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
