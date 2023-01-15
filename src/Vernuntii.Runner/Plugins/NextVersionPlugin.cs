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
using Vernuntii.Plugins.CommandLine;
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
    [ImportPlugin<NextVersionDaemonPlugin>(TryRegister = true)]
    public class NextVersionPlugin : Plugin, INextVersionPlugin
    {
        /// <inheritdoc/>
        public ICommandSeat Command { get; }

        private readonly Stopwatch _loadingVersionStopwatch = new();
        private readonly IPluginRegistry _pluginRegistry;
        private readonly SharedOptionsPlugin _sharedOptions = null!;
        private readonly ILogger _logger;

        private VersionPresentationKind _presentationKind;
        private VersionPresentationParts? _presentationParts;
        private VersionPresentationView _presentationView;
        private bool _emptyCaches;

        private readonly IServicesPlugin _globalServiceProvider;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="pluginRegistry"></param>
        /// <param name="sharedOptions"></param>
        /// <param name="commandLine"></param>
        /// <param name="globalServiceProvider"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NextVersionPlugin(
            IPluginRegistry pluginRegistry,
            SharedOptionsPlugin sharedOptions,
            ICommandLinePlugin commandLine,
            IServicesPlugin globalServiceProvider,
            ILogger<NextVersionPlugin> logger)
        {
            _pluginRegistry = pluginRegistry ?? throw new ArgumentNullException(nameof(pluginRegistry));
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _globalServiceProvider = globalServiceProvider ?? throw new ArgumentNullException(nameof(globalServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (commandLine is null) {
                throw new ArgumentNullException(nameof(commandLine));
            }

            Command = commandLine.RequestRootCommandSeat();
        }

        private async ValueTask<IServiceScope> CreateServiceScope()
        {
            var scope = _globalServiceProvider.CreateScope();
            await Events.EmitAsync(NextVersionEvents.OnCreatedScopedServiceProvider, scope.ServiceProvider).ConfigureAwait(false);
            return scope;
        }

        private async Task CalculateNextVersion()
        {
            if (_presentationParts is null) {
                throw new InvalidOperationException($"The presentation parts should have been set during the {nameof(CommandLineEvents.OnSealRootCommand)} event");
            }

            IVersionCache versionCache;

            if (_pluginRegistry.TryGetPlugin<IVersionCachePlugin>(out var versionCachePlugin) && versionCachePlugin.IsCacheUpToDate) {
                versionCache = versionCachePlugin.VersionCache;
            } else {
                await Events.EmitAsync(ServicesEvents.CreateServiceProvider).ConfigureAwait(false);
                using var calculationServiceProviderScope = await CreateServiceScope().ConfigureAwait(false);
                var calculationServiceProvider = calculationServiceProviderScope.ServiceProvider;
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

            var versionCacheString = new VersionCacheStringBuilder(versionCache)
                .UsePresentationKind(_presentationKind)
                .UsePresentationParts(_presentationParts)
                .UsePresentationView(_presentationView)
                .ToString();

            var nextVersionResult = new NextVersionResult(versionCacheString, versionCache);
            await Events.EmitAsync(NextVersionEvents.OnCalculatedNextVersion, nextVersionResult).ConfigureAwait(false);

            Console.Write(versionCacheString);
            _logger.LogInformation("Loaded version {Version} in {LoadTime}", versionCache.Version, _loadingVersionStopwatch.Elapsed.ToSecondsString());
        }

        private async Task<int> HandleRootCommandInvocation()
        {
            var commandInvocation = new CommandInvocation();
            await Events.EmitAsync(NextVersionEvents.OnInvokeNextVersionCommand, commandInvocation).ConfigureAwait(false);

            if (commandInvocation.IsHandled) {
                return (int)ExitCode.Success;
            }

            await Events.EmitAsync(NextVersionEvents.CalculateNextVersion).ConfigureAwait(false);

            var commandInvocationResult = new CommandInvocationResult();
            await Events.EmitAsync(NextVersionEvents.OnInvokedNextVersionCommand, commandInvocationResult).ConfigureAwait(false);
            return commandInvocationResult.ExitCode ?? (int)ExitCode.Success;
        }

        private void InitializeCommandLine()
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


            Command.SetHandler(HandleRootCommandInvocation);
            Command.Add(presentationKindOption);
            Command.Add(presentationPartsOption);
            Command.Add(presentationViewOption);
            Command.Add(emptyCachesOption);

            Events.Once(CommandLineEvents.OnSealRootCommand)
                .Subscribe(async _ => {
                    var versionPresentationContext = new VersionPresentationContext();
                    versionPresentationContext.ImportNextVersionRequirements(); // Adds next-version-specific defaults
                    await Events.EmitAsync(NextVersionEvents.OnConfigureVersionPresentation, versionPresentationContext).ConfigureAwait(false);
                    presentableParts = new VersionPresentationParts(versionPresentationContext.PresentableParts);
                });

            Events.Once(CommandLineEvents.ParsedCommandLineArguments)
                .Where(() => Command.IsSeatTaken)
                .Subscribe(parseResult => {
                    _presentationKind = parseResult.GetValueForOption(presentationKindOption);
                    _presentationParts = parseResult.GetValueForOption(presentationPartsOption);
                    _presentationView = parseResult.GetValueForOption(presentationViewOption);
                    _emptyCaches = parseResult.GetValueForOption(emptyCachesOption);
                });
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            InitializeCommandLine();

            Events.Every(LifecycleEvents.BeforeEveryRun).Subscribe(_ => _loadingVersionStopwatch.Restart());

            Events.OnceFirst(LifecycleEvents.BeforeEveryRun, CommandLineEvents.ParsedCommandLineArguments)
                .Where(() => Command.IsSeatTaken)
                .Subscribe(_ => Events.EmitAsync(VersionCacheEvents.CheckVersionCache));

            Events.Once(ServicesEvents.OnConfigureServices)
                .Subscribe(() => Events.EmitAsync(ConfigurationEvents.CreateConfiguration));

            Events.Once(ServicesEvents.OnConfigureServices)
                .Zip(ConfigurationEvents.OnCreatedConfiguration)
                .Subscribe(async result => {
                    var (services, configuration) = result;

                    await Events.EmitAsync(NextVersionEvents.OnConfigureServices, services).ConfigureAwait(false);

                    services
                        .TakeViewOfVernuntii()
                        .AddVersionIncrementer()
                        .AddVersionIncrementation(view => view
                            .TryOverrideStartVersion(configuration));

                    if (_sharedOptions.ShouldOverrideVersioningMode) {
                        services.TakeViewOfVernuntii().AddVersionIncrementation(view => view.UseVersioningMode(_sharedOptions.OverrideVersioningMode));
                    }
                });

            Events.Every(NextVersionEvents.CalculateNextVersion).Subscribe(CalculateNextVersion);
        }

        /// <summary>
        /// The result of the command invocation after the next version has been calculated.
        /// </summary>
        public class CommandInvocation
        {
            /// <summary>
            /// If you set it to <see langword="false"/>, the command invocation won't be handled.
            /// </summary>
            public bool IsHandled { get; set; }
        }

        /// <summary>
        /// The result of the command invocation after the next version has been calculated.
        /// </summary>
        public class CommandInvocationResult
        {
            /// <summary>
            /// The exit code, that will returned.
            /// </summary>
            public int? ExitCode { get; set; }
        }
    }
}
