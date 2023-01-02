using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Collections;
using Vernuntii.CommandLine;
using Vernuntii.Diagnostics;
using Vernuntii.Extensions;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.Runner;
using Vernuntii.VersionPersistence;
using Vernuntii.VersionPersistence.Presentation;
using Vernuntii.VersionPersistence.Presentation.Serializers;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Plugin that produces the next version and writes it to console.
    /// </summary>
    [ImportPlugin<SharedOptionsPlugin>(TryRegister = true)]
    public class NextVersionPlugin : Plugin, INextVersionPlugin
    {
        /// <inheritdoc/>
        public int? ExitCodeOnSuccess { get; set; }

        private readonly Stopwatch _loadingVersionStopwatch = new();
        private readonly IPluginRegistry _pluginRegistry;
        private readonly SharedOptionsPlugin _sharedOptions = null!;
        private readonly ILogger _logger;
        private readonly ICommandLinePlugin _commandLine;
        private ICommandHandler? _commandHandler;

        private VersionPresentationKind _presentationKind;
        private VersionPresentationParts? _presentationParts;
        private VersionPresentationView _presentationView;
        private bool _emptyCaches;

        private readonly IServicesPlugin _globalServiceProvider;

        public NextVersionPlugin(
            IPluginRegistry pluginRegistry,
            SharedOptionsPlugin sharedOptions,
            ICommandLinePlugin commandLine,
            IServicesPlugin globalServiceProvider,
            ILogger<NextVersionPlugin> logger)
        {
            _pluginRegistry = pluginRegistry ?? throw new ArgumentNullException(nameof(pluginRegistry));
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            _globalServiceProvider = globalServiceProvider ?? throw new ArgumentNullException(nameof(globalServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async ValueTask<IServiceScope> CreateServiceScope()
        {
            var scope = _globalServiceProvider.CreateScope();
            await Events.FulfillAsync(NextVersionEvents.CreatedScopedServiceProvider, scope.ServiceProvider).ConfigureAwait(false);
            return scope;
        }

        private async Task<int> HandleRootCommandInvocation()
        {
            if (_presentationParts is null) {
                throw new InvalidOperationException($"The presentation parts should have been set during the {nameof(CommandLineEvents.SealRootCommand)} event");
            }

            IVersionCache versionCache;

            if (_pluginRegistry.TryGetPlugin<IVersionCachePlugin>(out var versionCachePlugin) && versionCachePlugin.IsCacheUpToDate) {
                versionCache = versionCachePlugin.VersionCache;
            } else {
                using var calculationServiceProviderScope = await CreateServiceScope().ConfigureAwait(false);
                var calculationServiceProvider = calculationServiceProviderScope.ServiceProvider;
                //var repository = calculationServiceProvider.GetRequiredService<IRepository>();
                var versionCalculation = calculationServiceProvider.GetRequiredService<IVersionIncrementation>();
                var incrementedVersion = versionCalculation.GetIncrementedVersion();

                var versionCacheManager = calculationServiceProvider.GetService<IVersionCacheManager>();

                var versionCacheDataTuples = new VersionCacheDataTuples();
                versionCacheDataTuples.AddData(VersionCacheParts.Version, incrementedVersion);

                var versionCacheDataTuplesEnrichers = calculationServiceProvider.GetService<IEnumerable<IVersionCacheDataTuplesEnricher>>();

                if (versionCacheDataTuplesEnrichers is not null) {
                    foreach (var enricher in versionCacheDataTuplesEnrichers) {
                        enricher.Enrich(versionCacheDataTuples);
                    }
                }

                if (versionCacheManager is not null) {
                    versionCache = versionCacheManager.RecacheCache(
                        incrementedVersion,
                        new ImmutableVersionCacheDataTuples(versionCacheDataTuples));
                } else {
                    _logger.LogWarning("A version cache manager was not part of the service provider, so next calculation(s) won't profit from a version cache");

                    versionCache = new VersionCache(
                        incrementedVersion,
                        new ImmutableVersionCacheDataTuples(versionCacheDataTuples),
                        skipDataLookup: false);
                }
            }

            await Events.FulfillAsync(NextVersionEvents.CalculatedNextVersion, versionCache.Version).ConfigureAwait(false);

            var formattedVersion = new VersionCacheStringBuilder(versionCache)
                .UsePresentationKind(_presentationKind)
                .UsePresentationParts(_presentationParts)
                .UsePresentationView(_presentationView)
                .ToString();

            Console.Write(formattedVersion);
            _logger.LogInformation("Loaded version {Version} in {LoadTime}", versionCache.Version, _loadingVersionStopwatch.Elapsed.ToSecondsString());

            return ExitCodeOnSuccess ?? (int)ExitCode.Success;
        }

        private void ConfigureCommandLine()
        {
            var presentationPartsOptionLongAlias = "--presentation-parts";

            var presentationKindAndPartsHelpText =
                $" If using \"{nameof(VersionPresentationKind.Value)}\" only one part of the presentation can be displayed" +
                $" (e.g. {presentationPartsOptionLongAlias} {nameof(VersionCacheParts.Minor)})." +
                $" If using \"{nameof(VersionPresentationKind.Complex)}\" one or more parts of the presentation can be displayed" +
                $" (e.g. {presentationPartsOptionLongAlias} {nameof(VersionCacheParts.Minor)},{VersionCacheParts.Major}).";

            Option<VersionPresentationKind> presentationKindOption = new(new[] { "--presentation-kind" }, () =>
                VersionCacheStringBuilder.DefaultPresentationKind) {
                Description = "The kind of presentation." + presentationKindAndPartsHelpText
            };

            IReadOnlyContentwiseCollection<VersionCachePart>? presentableParts = null;

            Option<VersionPresentationParts> presentationPartsOption = new(
                new[] { presentationPartsOptionLongAlias },
                new ParseArgument<VersionPresentationParts>(argumentResult => {
                    var presentationPartAllowlist = VersionPresentationParts.AllowAll(presentableParts, VersionCachePartEqualityComparer.InvariantCultureIgnoreCase);
                    var parts = argumentResult.ParseList(VersionCachePart.New, presentationPartAllowlist);

                    return VersionPresentationParts.HasAllPart(parts)
                        ? VersionPresentationParts.Of(presentableParts)
                        : new VersionPresentationParts(parts);
                })) {
                Description = "The parts of the presentation to be displayed." + presentationKindAndPartsHelpText
            };

            presentationPartsOption.SetDefaultValue(VersionCacheStringBuilder.DefaultPresentationPart);

            Option<VersionPresentationView> presentationViewOption = new(new[] { "--presentation-view" }, () =>
                VersionCacheStringBuilder.DefaultPresentationSerializer) {
                Description = "The view of presentation."
            };

            Option<bool> emptyCachesOption = new(new string[] { "--empty-caches" }) {
                Description = "Empties all caches where version informations are stored. This happens before the cache process itself."
            };

            _commandHandler = _commandLine.RootCommand.SetHandler(HandleRootCommandInvocation);
            _commandLine.RootCommand.Add(presentationKindOption);
            _commandLine.RootCommand.Add(presentationPartsOption);
            _commandLine.RootCommand.Add(presentationViewOption);
            _commandLine.RootCommand.Add(emptyCachesOption);

            Events.Earliest(CommandLineEvents.SealRootCommand)
                .Subscribe(async command => {
                    var versionPresentationContext = new VersionPresentationContext();
                    versionPresentationContext.ImportNextVersionRequirements(); // Adds next-version-agnostic defaults
                    await Events.FulfillAsync(NextVersionEvents.ConfigureVersionPresentation, versionPresentationContext);
                    presentableParts = new VersionPresentationParts(versionPresentationContext.PresentableParts);
                });

            Events.Earliest(CommandLineEvents.ParsedCommandLineArguments).Subscribe(parseResult => {
                if (parseResult.CommandResult.Command.Handler != _commandHandler) {
                    return Task.CompletedTask;
                }

                _presentationKind = parseResult.GetValueForOption(presentationKindOption);
                _presentationParts = parseResult.GetValueForOption(presentationPartsOption);
                _presentationView = parseResult.GetValueForOption(presentationViewOption);
                _emptyCaches = parseResult.GetValueForOption(emptyCachesOption);

                // Check version cache
                return Events.FulfillAsync(VersionCacheEvents.CheckVersionCache);
            });
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            ConfigureCommandLine();

            Events.Every(LifecycleEvents.BeforeEveryRun).Subscribe(_ => _loadingVersionStopwatch.Restart());

            Events.Earliest(ServicesEvents.ConfigureServices)
                .Zip(ConfigurationEvents.CreatedConfiguration)
                .Subscribe(async result => {
                    var (services, configuration) = result;

                    await Events.FulfillAsync(NextVersionEvents.ConfigureServices, services).ConfigureAwait(false);

                    services
                        .TakeViewOfVernuntii()
                        .AddVersionIncrementer()
                        .AddVersionIncrementation(view => view
                            .TryOverrideStartVersion(configuration));

                    if (_sharedOptions.ShouldOverrideVersioningMode) {
                        services.TakeViewOfVernuntii().AddVersionIncrementation(view => view.UseVersioningMode(_sharedOptions.OverrideVersioningMode));
                    }
                });
        }
    }
}
