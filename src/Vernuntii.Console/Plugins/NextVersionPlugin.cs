using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Autofac;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Git;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionCaching;
using Vernuntii.VersionPresentation;
using Vernuntii.VersionPresentation.Serializers;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Plugin that produces the next version and writes it to console.
    /// </summary>
    public class NextVersionPlugin : Plugin, INextVersionPlugin
    {
        /// <inheritdoc/>
        public int? ExitCodeOnSuccess { get; set; }

        private readonly Stopwatch _loadingVersionStopwatch = new();
        private readonly SharedOptionsPlugin _sharedOptions = null!;
        private readonly IVersionCacheCheckPlugin _versionCacheCheckPlugin = null!;
        private readonly ILogger _logger;
        private readonly ICommandLinePlugin _commandLine;
        private IConfiguration _configuration = null!;
        private ICommandHandler? _commandHandler;

        #region command line options

        private const string presentationPartsOptionLongAlias = "--presentation-parts";
        private const string presentationKindAndPartsHelpText =
            $" If using \"{nameof(VersionPresentationKind.Value)}\" only one part of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(VersionPresentationPart.Minor)})." +
            $" If using \"{nameof(VersionPresentationKind.Complex)}\" one or more parts of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(VersionPresentationPart.Minor)},{nameof(VersionPresentationPart.Major)}).";

        private readonly Option<VersionPresentationKind> _presentationKindOption = new(new[] { "--presentation-kind" }, () =>
            VersionPresentationStringBuilder.DefaultPresentationKind) {
            Description = "The kind of presentation." + presentationKindAndPartsHelpText
        };

        private readonly Option<VersionPresentationPart> _presentationPartsOption = new(new[] { presentationPartsOptionLongAlias }, () =>
            VersionPresentationStringBuilder.DefaultPresentationPart) {
            Description = "The parts of the presentation to be displayed." + presentationKindAndPartsHelpText
        };

        private readonly Option<VersionPresentationView> _presentationViewOption = new(new[] { "--presentation-view" }, () =>
            VersionPresentationStringBuilder.DefaultPresentationSerializer) {
            Description = "The view of presentation."
        };

        private readonly Option<bool> _emptyCachesOption = new(new string[] { "--empty-caches" }) {
            Description = "Empties all caches where version informations are stored. This happens before the cache process itself."
        };

        #endregion

        private VersionPresentationKind _presentationKind;
        private VersionPresentationPart _presentationParts;
        private VersionPresentationView _presentationView;
        private bool _emptyCaches;

        private ILifetimeScopedServiceProvider _globalServiceProvider = null!;

        public NextVersionPlugin(
            SharedOptionsPlugin sharedOptions,
            IVersionCacheCheckPlugin versionCacheCheckPlugin,
            ICommandLinePlugin commandLine,
            ILogger<NextVersionPlugin> logger)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _versionCacheCheckPlugin = versionCacheCheckPlugin ?? throw new ArgumentNullException(nameof(versionCacheCheckPlugin));
            _logger = logger;
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
        }

        private ILifetimeScopedServiceProvider CreateCalculationServiceProvider()
        {
            var calculationServiceProvider = _globalServiceProvider.CreateScope(services => {
                Events.Publish(NextVersionEvents.CreatedCalculationServices, services);

                services.ScopeToVernuntii(features => features
                    .AddVersionIncrementer()
                    .AddVersionIncrementation(features => features
                        .TryOverrideStartVersion(_configuration)));

                if (_sharedOptions.ShouldOverrideVersioningMode) {
                    services.ScopeToVernuntii(features => features
                        .AddVersionIncrementation(features => features
                            .UseVersioningMode(_sharedOptions.OverrideVersioningMode)));
                }

                Events.Publish(NextVersionEvents.ConfiguredCalculationServices, services);
            });

            Events.Publish(NextVersionEvents.CreatedCalculationServiceProvider, calculationServiceProvider);
            return calculationServiceProvider;
        }

        private int OnInvokeCommand()
        {
            IVersionCache versionCache;

            if (_versionCacheCheckPlugin.IsCacheUpToDate) {
                versionCache = _versionCacheCheckPlugin.VersionCache;
            } else {
                using var calculationServiceProvider = CreateCalculationServiceProvider();
                var repository = calculationServiceProvider.GetRequiredService<IRepository>();
                var versionCalculation = calculationServiceProvider.GetRequiredService<IVersionIncrementation>();
                var versionCacheManager = calculationServiceProvider.GetRequiredService<IVersionCacheManager>();

                var newVersion = versionCalculation.GetIncrementedVersion();
                var newBranch = repository.GetActiveBranch();

                // get cache or calculate version.
                versionCache = versionCacheManager.RecacheCache(
                    newVersion,
                    newBranch);
            }

            Events.Publish(NextVersionEvents.CalculatedNextVersion, versionCache);

            var formattedVersion = new VersionPresentationStringBuilder(versionCache)
                .UsePresentationKind(_presentationKind)
                .UsePresentationPart(_presentationParts)
                .UsePresentationView(_presentationView)
                .BuildString();

            System.Console.Write(formattedVersion);
            _logger.LogInformation("Loaded version {Version} in {LoadTime}", versionCache.Version, $"{_loadingVersionStopwatch.Elapsed.ToString("s\\.ff", CultureInfo.InvariantCulture)}s");

            return ExitCodeOnSuccess ?? (int)ExitCode.Success;
        }

        private void OnConfigureCommandLine(ICommandLinePlugin cli)
        {
            _commandHandler = cli.RootCommand.SetHandler(OnInvokeCommand);
            cli.RootCommand.Add(_presentationKindOption);
            cli.RootCommand.Add(_presentationPartsOption);
            cli.RootCommand.Add(_presentationViewOption);
            cli.RootCommand.Add(_emptyCachesOption);
        }

        private void OnParsedCommandLine(ParseResult parseResult)
        {
            if (parseResult.CommandResult.Command.Handler != _commandHandler) {
                return;
            }

            _presentationKind = parseResult.GetValueForOption(_presentationKindOption);
            _presentationParts = parseResult.GetValueForOption(_presentationPartsOption);
            _presentationView = parseResult.GetValueForOption(_presentationViewOption);
            _emptyCaches = parseResult.GetValueForOption(_emptyCachesOption);

            // Check version cache.
            Events.Publish(VersionCacheCheckEvents.CreateVersionCacheManager);
            Events.Publish(VersionCacheCheckEvents.CheckVersionCache);

            Events.Publish(ConfigurationEvents.CreateConfiguration);
            Events.Publish(GlobalServicesEvents.CreateServiceProvider);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            OnConfigureCommandLine(_commandLine);

            Events.Subscribe(LifecycleEvents.BeforeEveryRun, _loadingVersionStopwatch.Restart);

            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, OnParsedCommandLine);

            Events.SubscribeOnce(
                ConfigurationEvents.CreatedConfiguration,
                configuration => _configuration = configuration);

            Events.SubscribeOnce(
                GlobalServicesEvents.ConfigureServices,
                sp => {
                    Events.Publish(NextVersionEvents.ConfigureGlobalServices, sp);
                    Events.Publish(NextVersionEvents.ConfiguredGlobalServices, sp);
                });

            Events.SubscribeOnce(
                GlobalServicesEvents.CreatedServiceProvider,
                sp => _globalServiceProvider = sp);
        }
    }
}
