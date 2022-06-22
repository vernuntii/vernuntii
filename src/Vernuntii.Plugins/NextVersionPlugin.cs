using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Autofac;
using Vernuntii.Caching;
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

        private Stopwatch _loadingVersionStopwatch = Stopwatch.StartNew();
        private ILoggingPlugin _loggingPlugin = null!;
        private SharedOptionsPlugin _sharedOptions = null!;
        private IVersionCacheCheckPlugin _versionCacheCheckPlugin = null!;
        private ILogger _logger = null!;
        private IConfiguration _configuration = null!;

        #region command line options

        const string presentationPartsOptionLongAlias = "--presentation-parts";

        const string presentationKindAndPartsHelpText =
            $" If using \"{nameof(VersionPresentationKind.Value)}\" only one part of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(VersionPresentationPart.Minor)})." +
            $" If using \"{nameof(VersionPresentationKind.Complex)}\" one or more parts of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(VersionPresentationPart.Minor)},{nameof(VersionPresentationPart.Major)}).";

        private Option<VersionPresentationKind> _presentationKindOption = new Option<VersionPresentationKind>(new[] { "--presentation-kind" }, () =>
            VersionPresentationStringBuilder.DefaultPresentationKind) {
            Description = "The kind of presentation." + presentationKindAndPartsHelpText
        };

        private Option<VersionPresentationPart> _presentationPartsOption = new Option<VersionPresentationPart>(new[] { presentationPartsOptionLongAlias }, () =>
            VersionPresentationStringBuilder.DefaultPresentationPart) {
            Description = "The parts of the presentation to be displayed." + presentationKindAndPartsHelpText
        };

        private Option<VersionPresentationView> _presentationViewOption = new Option<VersionPresentationView>(new[] { "--presentation-view" }, () =>
            VersionPresentationStringBuilder.DefaultPresentationSerializer) {
            Description = "The view of presentation."
        };

        private Option<bool> _emptyCachesOption = new Option<bool>(new string[] { "--empty-caches" }) {
            Description = "Empties all caches where version informations are stored. This happens before the cache process itself."
        };

        #endregion

        private VersionPresentationKind _presentationKind;
        private VersionPresentationPart _presentationParts;
        private VersionPresentationView _presentationView;
        private bool _emptyCaches;

        private ILifetimeScopedServiceProvider _globalServiceProvider = null!;

        /// <inheritdoc/>
        protected override async ValueTask OnRegistrationAsync(RegistrationContext registrationContext) =>
            await registrationContext.PluginRegistry.TryRegisterAsync<SharedOptionsPlugin>();

        private ILifetimeScopedServiceProvider CreateCalculationServiceProvider()
        {
            var calculationServiceProvider = _globalServiceProvider.CreateScope(services => {
                Events.Publish(NextVersionEvents.CreatedCalculationServices, services);

                services.ConfigureVernuntii(features => features
                    .AddSingleVersionCalculator()
                    .AddSingleVersionCalculation(features => features
                        .TryOverrideStartVersion(_configuration)));

                if (_sharedOptions.ShouldOverrideVersioningMode) {
                    services.ConfigureVernuntii(features => features
                        .AddSingleVersionCalculation(features => features
                            .UseVersioningMode(_sharedOptions.OverrideVersioningMode)));
                }

                Events.Publish(NextVersionEvents.ConfiguredCalculationServices, services);
            });

            Events.Publish(NextVersionEvents.CreatedCalculationServiceProvider, calculationServiceProvider);
            return calculationServiceProvider;
        }

        private int ProduceVersionOutput()
        {
            try {
                IVersionCache versionCache;

                if (_versionCacheCheckPlugin.IsCacheUpToDate) {
                    versionCache = _versionCacheCheckPlugin.VersionCache;
                } else {
                    using var calculationServiceProvider = CreateCalculationServiceProvider();
                    var repository = calculationServiceProvider.GetRequiredService<IRepository>();
                    var versionCalculation = calculationServiceProvider.GetRequiredService<ISingleVersionCalculation>();
                    var versionCacheManager = calculationServiceProvider.GetRequiredService<IVersionCacheManager>();

                    var newVersion = versionCalculation.GetVersion();
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
                _logger.LogInformation("Loaded version {Version} in {LoadTime}", versionCache.Version, $"{_loadingVersionStopwatch.Elapsed.ToString("s\\.f", CultureInfo.InvariantCulture)}s");

                return ExitCodeOnSuccess ?? (int)ExitCode.Success;
            } catch (Exception error) {
                UnwrapError(ref error);
                _logger.LogCritical(error, $"{nameof(Vernuntii)} stopped due to an exception.");
                return (int)ExitCode.Failure;

                // Unwraps error to make the actual error more valuable for casual users.
                [Conditional("RELEASE")]
                static void UnwrapError(ref Exception error)
                {
                    if (error is DependencyResolutionException dependencyResolutionException
                        && dependencyResolutionException.InnerException is not null) {
                        error = dependencyResolutionException.InnerException;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnAfterRegistration()
        {
            Plugins.FirstLazy<ICommandLinePlugin>().Registered += plugin => {
                plugin.SetRootCommandHandler(ProduceVersionOutput);
                plugin.RootCommand.Add(_presentationKindOption);
                plugin.RootCommand.Add(_presentationPartsOption);
                plugin.RootCommand.Add(_presentationViewOption);
                plugin.RootCommand.Add(_emptyCachesOption);
            };

            _loggingPlugin = Plugins.First<ILoggingPlugin>();
            _sharedOptions = Plugins.First<SharedOptionsPlugin>();
            _versionCacheCheckPlugin = Plugins.First<IVersionCacheCheckPlugin>();
        }

        /// <inheritdoc/>
        protected override void OnEvents()
        {
            Events.Subscribe(LifecycleEvents.BeforeNextRun, _loadingVersionStopwatch.Restart);

            Events.SubscribeOnce(
                LoggingEvents.EnabledLoggingInfrastructure,
                plugin => _logger = plugin.CreateLogger<NextVersionPlugin>());

            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, parseResult => {
                _presentationKind = parseResult.GetValueForOption(_presentationKindOption);
                _presentationParts = parseResult.GetValueForOption(_presentationPartsOption);
                _presentationView = parseResult.GetValueForOption(_presentationViewOption);
                _emptyCaches = parseResult.GetValueForOption(_emptyCachesOption);
            });

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
