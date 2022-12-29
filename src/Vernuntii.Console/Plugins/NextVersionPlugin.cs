using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Git;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
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

        private readonly IServicesPlugin _globalServiceProvider;

        public NextVersionPlugin(
            SharedOptionsPlugin sharedOptions,
            IVersionCacheCheckPlugin versionCacheCheckPlugin,
            ICommandLinePlugin commandLine,
            IServicesPlugin globalServiceProvider,
            ILogger<NextVersionPlugin> logger)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _versionCacheCheckPlugin = versionCacheCheckPlugin ?? throw new ArgumentNullException(nameof(versionCacheCheckPlugin));
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            _globalServiceProvider = globalServiceProvider ?? throw new ArgumentNullException(nameof(globalServiceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async ValueTask<IServiceScope> CreateServiceScope()
        {
            var scope = _globalServiceProvider.CreateScope();
            await Events.FulfillAsync(NextVersionEvents.CreatedScopedServiceProvider, scope.ServiceProvider);
            return scope;
        }

        private async Task<int> OnInvokeCommand()
        {
            IVersionCache versionCache;

            if (_versionCacheCheckPlugin.IsCacheUpToDate) {
                versionCache = _versionCacheCheckPlugin.VersionCache;
            } else {
                using var calculationServiceProviderScope = await CreateServiceScope();
                var calculationServiceProvider = calculationServiceProviderScope.ServiceProvider;
                var repository = calculationServiceProvider.GetRequiredService<IRepository>();
                var versionCalculation = calculationServiceProvider.GetRequiredService<IVersionIncrementation>();
                var versionCacheManager = calculationServiceProvider.GetRequiredService<IVersionCacheManager>();

                var newVersion = versionCalculation.GetIncrementedVersion();
                var newBranch = repository.GetActiveBranch();

                // Get cache or calculate version.
                versionCache = versionCacheManager.RecacheCache(
                    newVersion,
                    newBranch);
            }

            await Events.FulfillAsync(NextVersionEvents.CalculatedNextVersion, versionCache);

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

        private async ValueTask OnParsedCommandLine(ParseResult parseResult)
        {
            if (parseResult.CommandResult.Command.Handler != _commandHandler) {
                return;
            }

            _presentationKind = parseResult.GetValueForOption(_presentationKindOption);
            _presentationParts = parseResult.GetValueForOption(_presentationPartsOption);
            _presentationView = parseResult.GetValueForOption(_presentationViewOption);
            _emptyCaches = parseResult.GetValueForOption(_emptyCachesOption);

            // Check version cache
            await Events.FulfillAsync(VersionCacheCheckEvents.CheckVersionCache);
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            OnConfigureCommandLine(_commandLine);
            Events.Every(LifecycleEvents.BeforeEveryRun).Subscribe(_ => _loadingVersionStopwatch.Restart()).DisposeWhenDisposing(this);
            Events.Earliest(CommandLineEvents.ParsedCommandLineArguments).Subscribe(OnParsedCommandLine).DisposeWhenDisposing(this);

            Events.Earliest(ServicesEvents.ConfigureServices)
                .Zip(ConfigurationEvents.CreatedConfiguration)
                .Subscribe(async result => {
                    var (services, configuration) = result;

                    await Events.FulfillAsync(NextVersionEvents.ConfigureServices, services);

                    services.ScopeToVernuntii(features => features
                        .AddVersionIncrementer()
                        .AddVersionIncrementation(features => features
                            .TryOverrideStartVersion(configuration)));

                    if (_sharedOptions.ShouldOverrideVersioningMode) {
                        services.ScopeToVernuntii(features => features
                            .AddVersionIncrementation(features => features
                                .UseVersioningMode(_sharedOptions.OverrideVersioningMode)));
                    }
                });
        }
    }
}
