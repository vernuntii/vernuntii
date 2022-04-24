using System.CommandLine;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vernuntii.Autofac;
using Vernuntii.Console;
using Vernuntii.Extensions;
using Vernuntii.Extensions.VersionFoundation;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionPresentation;
using Vernuntii.VersionPresentation.Serializers;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// Plugin that produces the next version and writes it to console.
    /// </summary>
    public class NextVersionPlugin : Plugin, INextVersionPlugin
    {
        private ILoggingPlugin _loggingPlugin = null!;
        private NextVersionOptionsPlugin _options = null!;
        private ILogger _logger = null!;
        private IConfiguration _configuration = null!;

        #region command line options

        const string presentationPartsOptionLongAlias = "--presentation-parts";

        const string presentationKindAndPartsHelpText =
            $" If using \"{nameof(SemanticVersionPresentationKind.Value)}\" only one part of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(SemanticVersionPresentationPart.Minor)})." +
            $" If using \"{nameof(SemanticVersionPresentationKind.Complex)}\" one or more parts of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(SemanticVersionPresentationPart.Minor)},{nameof(SemanticVersionPresentationPart.Major)}).";

        private Option<SemanticVersionPresentationKind> _presentationKindOption = new Option<SemanticVersionPresentationKind>(new[] { "--presentation-kind" }, () =>
            SemanticVersionPresentationStringBuilder.DefaultPresentationKind) {
            Description = "The kind of presentation." + presentationKindAndPartsHelpText
        };

        private Option<SemanticVersionPresentationPart> _presentationPartsOption = new Option<SemanticVersionPresentationPart>(new[] { presentationPartsOptionLongAlias }, () =>
            SemanticVersionPresentationStringBuilder.DefaultPresentationPart) {
            Description = "The parts of the presentation to be displayed." + presentationKindAndPartsHelpText
        };

        private Option<SemanticVersionPresentationView> _presentationViewOption = new Option<SemanticVersionPresentationView>(new[] { "--presentation-view" }, () =>
            SemanticVersionPresentationStringBuilder.DefaultPresentationSerializer) {
            Description = "The view of presentation."
        };

        private Option<bool> _duplicateVersionFailsOption = new Option<bool>(new string[] { "--duplicate-version-fails" }) {
            Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
        };

        private const string CacheIdOptionAlias = "--cache-id";

        private Option<string> _cacheIdOption = new Option<string>(new string[] { CacheIdOptionAlias }) {
            Description = "The non-case-sensitive cache id is used to cache the version informations once and load them on next accesses." +
                $" If {CacheIdOptionAlias} is not specified it is implicitly the internal cache id: {SemanticVersionFoundationProviderOptions.DefaultInternalCacheId}"
        };

        private Option<TimeSpan?> _cacheCreationRetentionTimeOption = new Option<TimeSpan?>(new string[] { "--cache-creation-retention-time" }, parseArgument: result => {
            if (result.Tokens.Count == 0 || result.Tokens[0].Value == string.Empty) {
                return null; // = internal default is taken
            }

            return TimeSpan.Parse(result.Tokens[0].Value, CultureInfo.InvariantCulture);
        }) {
            Description = "The cache retention time since creation. If the time span since creation is greater than then" +
                " the at creation specified retention time then the version informations is reloaded. Null or empty means the" +
                $" default creation retention time of {SemanticVersionFoundationProviderOptions.DefaultCacheCreationRetentionTime.TotalHours}" +
                " hours is used.",
            Arity = ArgumentArity.ZeroOrOne
        };

        private Option<TimeSpan?> _cacheLastAccessRetentionTimeOption = new Option<TimeSpan?>(new string[] { "--cache-last-access-retention-time" }, parseArgument: result => {
            if (result.Tokens.Count == 0 || result.Tokens[0].Value == string.Empty) {
                return null; // = feature won't be used
            }

            return TimeSpan.Parse(result.Tokens[0].Value, CultureInfo.InvariantCulture);
        }) {
            Description = "The cache retention time since last access. If the time span since last access is greater than the" +
                " retention time then the version informations is reloaded. Null or empty means this feature is disabled except" +
                $" if the cache id is implictly or explictly equals to the internal cache id, then the default last access retention time of" +
                $" {SemanticVersionFoundationProviderOptions.DefaultInternalCacheLastAccessRetentionTime.ToString("s\\.f", CultureInfo.InvariantCulture)}s" +
                " is used.",
            Arity = ArgumentArity.ZeroOrOne
        };

        private Option<bool> _emptyCachesOption = new Option<bool>(new string[] { "--empty-caches" }) {
            Description = "Empties all caches where version informations are stored. This happens before the cache process itself."
        };

        #endregion

        private SemanticVersionPresentationKind _presentationKind;
        private SemanticVersionPresentationPart _presentationParts;
        private SemanticVersionPresentationView _presentationView;
        private bool _duplicateVersionFails;
        private string? _cacheId;
        private TimeSpan? _cacheCreationRetentionTime;
        private TimeSpan? _cacheLastAccessRetentionTime;
        private bool _emptyCaches;

        /// <inheritdoc/>
        protected override void OnRegistration() =>
            PluginRegistry.TryRegister<NextVersionOptionsPlugin>();

        private int ProduceVersionOutput()
        {
            try {
                IServiceCollection globalServices = new ServiceCollection();
                EventAggregator.PublishEvent(NextVersionEvents.CreatedGlobalServices.Discriminator, globalServices);

                globalServices.AddLogging(builder => _loggingPlugin.Bind(builder));
                EventAggregator.PublishEvent(NextVersionEvents.ConfiguredGlobalServices.Discriminator, globalServices);

                using var globalServiceProvider = globalServices.BuildLifetimeScopedServiceProvider();

                using var calculationServiceProvider = globalServiceProvider.CreateScope(services => {
                    EventAggregator.PublishEvent(NextVersionEvents.CreatedCalculationServices.Discriminator, services);

                    services.ConfigureVernuntii(features => features
                        .AddSemanticVersionCalculator()
                        .AddSemanticVersionCalculation(features => features
                            .TryOverrideStartVersion(_configuration))
                        .AddSemanticVersionFoundationProvider(options => options.EmptyCaches = _emptyCaches));

                    if (_options.ShouldOverrideVersioningMode) {
                        services.ConfigureVernuntii(features => features
                            .AddSemanticVersionCalculation(features => features
                                .UseVersioningMode(_options.OverrideVersioningMode.Value)));
                    }

                    EventAggregator.PublishEvent(NextVersionEvents.ConfiguredCalculationServices.Discriminator, services);
                });

                //var repository = globalServiceProvider.GetRequiredService<IRepository>();
                var presentationFoundationProvider = calculationServiceProvider.GetRequiredService<SemanticVersionFoundationProvider>();

                // get cache or calculate version.
                var presentationFoundation = presentationFoundationProvider.GetFoundation(
                    _cacheId,
                    creationRetentionTime: _cacheCreationRetentionTime,
                    lastAccessRetentionTime: _cacheLastAccessRetentionTime);

                EventAggregator.PublishEvent(NextVersionEvents.CalculatedNextVersion.Discriminator, presentationFoundation.Version);

                var formattedVersion = new SemanticVersionPresentationStringBuilder(presentationFoundation)
                    .UsePresentationKind(_presentationKind)
                    .UsePresentationPart(_presentationParts)
                    .UsePresentationView(_presentationView)
                    .BuildString();

                System.Console.WriteLine(formattedVersion);

                //if (_duplicateVersionFails && repository.HasCommitVersion(presentationFoundation.Version)) {
                //    return (int)ExitCode.VersionDuplicate;
                //}

                return (int)ExitCode.Success;
            } catch (Exception error) {
                _logger.LogCritical(error, "A fatal exception happenend, so the version calculation has been canceled.");
                return (int)ExitCode.Failure;
            }
        }

        /// <inheritdoc/>
        protected override void OnCompletedRegistration()
        {
            PluginRegistry.First<ICommandLinePlugin>().Registered += plugin => {
                plugin.SetRootCommandHandler(ProduceVersionOutput);
                plugin.RootCommand.Add(_presentationKindOption);
                plugin.RootCommand.Add(_presentationPartsOption);
                plugin.RootCommand.Add(_presentationViewOption);
                plugin.RootCommand.Add(_duplicateVersionFailsOption);
                plugin.RootCommand.Add(_cacheIdOption);
                plugin.RootCommand.Add(_cacheCreationRetentionTimeOption);
                plugin.RootCommand.Add(_cacheLastAccessRetentionTimeOption);
                plugin.RootCommand.Add(_emptyCachesOption);
            };

            _loggingPlugin = PluginRegistry.First<ILoggingPlugin>().Value;
            _options = PluginRegistry.First<NextVersionOptionsPlugin>().Value;
        }

        /// <inheritdoc/>
        protected override void OnEventAggregator()
        {
            SubscribeEvent(CommandLineEvents.ParsedCommandLineArgs.Discriminator, parseResult => {
                _presentationKind = parseResult.GetValueForOption(_presentationKindOption);
                _presentationParts = parseResult.GetValueForOption(_presentationPartsOption);
                _presentationView = parseResult.GetValueForOption(_presentationViewOption);
                _duplicateVersionFails = parseResult.GetValueForOption(_duplicateVersionFailsOption);
                _cacheId = parseResult.GetValueForOption(_cacheIdOption);
                _cacheCreationRetentionTime = parseResult.GetValueForOption(_cacheCreationRetentionTimeOption);
                _cacheLastAccessRetentionTime = parseResult.GetValueForOption(_cacheLastAccessRetentionTimeOption);
                _emptyCaches = parseResult.GetValueForOption(_emptyCachesOption);
            });

            SubscribeEvent(
                LoggingEvents.EnabledLoggingInfrastructure.Discriminator,
                plugin => _logger = plugin.CreateLogger<NextVersionPlugin>());

            SubscribeEvent(
                ConfigurationEvents.CreatedConfiguration.Discriminator,
                configuration => _configuration = configuration);
        }
    }
}
