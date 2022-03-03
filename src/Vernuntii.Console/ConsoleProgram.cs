using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Vernuntii.Autofac;
using Vernuntii.Configuration;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Extensions;
using Vernuntii.Extensions.BranchCases;
using Vernuntii.Extensions.VersionFoundation;
using Vernuntii.Git;
using Vernuntii.MessageVersioning;
using Vernuntii.VersionPresentation;
using Vernuntii.VersionPresentation.Serializers;
using CommandHandler = System.CommandLine.NamingConventionBinder.CommandHandler;

namespace Vernuntii.Console;

/// <summary>
/// The program class.
/// </summary>
public static class ConsoleProgram
{
    /// <summary>
    /// Runs console application.
    /// </summary>
    /// <param name="args">arguments</param>
    /// <param name="configureCommand"></param>
    /// <returns>exit code</returns>
    [SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
    public static async Task<int> RunAsync(string[] args, Action<RootCommand>? configureCommand = null)
    {
        /* If option is not specified, then do not log.
         * If value is not specified, then log on information level.
         * If value is specified, then log on specified log level.
         */
        var verboseOption = new Option<LogEventLevel?>(new[] { "--verbose", "-v" }, parseArgument: result => {
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

        var configPathOption = new Option<string?>(new[] { "--config-path", "-c" }) {
            Description = $"The configuration file path. JSON and YAML is allowed. If a directory is specified instead the configuration file" +
                $" {YamlConfigurationFileDefaults.YmlFileName}, {YamlConfigurationFileDefaults.YamlFileName} or {JsonConfigurationFileDefaults.JsonFileName}" +
                " (in each upward directory in this exact order) is searched at specified directory and above."
        };

        const string presentationPartsOptionLongAlias = "--presentation-parts";

        const string presentationKindAndPartsHelpText =
            $" If using \"{nameof(SemanticVersionPresentationKind.Value)}\" only one part of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(SemanticVersionPresentationPart.Minor)})." +
            $" If using \"{nameof(SemanticVersionPresentationKind.Complex)}\" one or more parts of the presentation can be displayed" +
            $" (e.g. {presentationPartsOptionLongAlias} {nameof(SemanticVersionPresentationPart.Minor)},{nameof(SemanticVersionPresentationPart.Major)}).";

        var presentationKindOption = new Option<SemanticVersionPresentationKind>(new[] { "--presentation-kind" }, () =>
            SemanticVersionPresentationStringBuilder.DefaultPresentationKind) {
            Description = "The kind of presentation." + presentationKindAndPartsHelpText
        };

        var presentationPartsOption = new Option<SemanticVersionPresentationPart>(new[] { presentationPartsOptionLongAlias }, () =>
            SemanticVersionPresentationStringBuilder.DefaultPresentationPart) {
            Description = "The parts of the presentation to be displayed." + presentationKindAndPartsHelpText
        };

        var presentationViewOption = new Option<SemanticVersionPresentationView>(new[] { "--presentation-view" }, () =>
            SemanticVersionPresentationStringBuilder.DefaultPresentationSerializer) {
            Description = "The view of presentation."
        };

        var overrideVersioningModeOption = new Option<VersioningModePreset?>(new[] { "--override-versioning-mode" });
        var overridePostPreReleaseOption = new Option<string?>(new[] { "--override-post-pre-release" });

        var duplicateVersionFailsOption = new Option<bool>(new string[] { "--duplicate-version-fails" }) {
            Description = $"If the produced version exists as tag already then the exit code will be {(int)ExitCode.VersionDuplicate}."
        };

        var cacheIdOptionAlias = "--cache-id";

        var cacheIdOption = new Option<string>(new string[] { cacheIdOptionAlias }) {
            Description = "The non-case-sensitive cache id is used to cache the version informations once and load them on next accesses." +
                $" If {cacheIdOptionAlias} is not specified it is implicitly the internal cache id: {SemanticVersionFoundationProviderOptions.DefaultInternalCacheId}"
        };

        var cacheCreationRetentionTimeOption = new Option<TimeSpan?>(new string[] { "--cache-creation-retention-time" }, parseArgument: result => {
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

        var cacheLastAccessRetentionTimeOption = new Option<TimeSpan?>(new string[] { "--cache-last-access-retention-time" }, parseArgument: result => {
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

        var emptyCachesOptions = new Option<bool>(new string[] { "--empty-caches" }) {
            Description = "Empties all caches where version informations are stored. This happens before the cache process itself."
        };

        var rootCommand = new RootCommand() {
            verboseOption,
            configPathOption,
            presentationPartsOption,
            presentationKindOption,
            presentationViewOption,
            overrideVersioningModeOption,
            overridePostPreReleaseOption,
            duplicateVersionFailsOption,
            cacheIdOption,
            cacheCreationRetentionTimeOption,
            cacheLastAccessRetentionTimeOption,
            emptyCachesOptions
        };

        // Parameter names are bound to naming convention, do not change!
        rootCommand.Handler = CommandHandler.Create((
            LogEventLevel? verbose,
            string? configPath,
            SemanticVersionPresentationKind presentationKind,
            SemanticVersionPresentationPart presentationParts,
            SemanticVersionPresentationView presentationView,
            VersioningModePreset? overrideVersioningMode,
            string? overridePostPreRelease,
            bool duplicateVersionFails,
            string? cacheId,
            TimeSpan? cacheCreationRetentionTime,
            TimeSpan? cacheLastAccessRetentionTime,
            bool emptyCaches) => {
                SetupLogger(verbose, out var loggerFactory, out var useSerilog);
                var logger = loggerFactory.CreateLogger(nameof(ConsoleProgram));
                var stopwatch = Stopwatch.StartNew();

                try {
                    var configuration = new ConventionalConfigurationBuilder()
                        .AddConventionalYamlFileFinder()
                        .AddConventionalJsonFileFinder()
                        .AddFileOrFirstConventionalFile(
                            configPath ?? Directory.GetCurrentDirectory(),
                            new[] {
                                YamlConfigurationFileDefaults.YmlFileName,
                                YamlConfigurationFileDefaults.YamlFileName,
                                JsonConfigurationFileDefaults.JsonFileName
                            },
                            out var addedFilePath,
                            configurator => configurator.UseGitDefaults())
                        .Build();

                    logger.LogInformation("Use configuration file: {ConfigurationFilePath}", addedFilePath);

                    using var gitServiceProvider = new ServiceCollection()
                        .AddLogging(builder => useSerilog(builder))
                        .ConfigureVernuntii(features => features
                           .ConfigureGit(features => features
                               .UseConfigurationDefaults(configuration)))
                        .BuildLifetimeScopedServiceProvider();

                    using var calculationServiceProvider = gitServiceProvider.CreateScope(services => {
                        services.ConfigureVernuntii(features => features
                            .ConfigureGit(features => features
                                .UseLatestCommitVersion()
                                .UseActiveBranchCaseDefaults()
                                .UseCommitMessagesProvider())
                            .AddSemanticVersionCalculator()
                            .AddSemanticVersionCalculation()
                            .AddSemanticVersionFoundationProvider(options => options.EmptyCaches = emptyCaches));

                        if (overrideVersioningMode == null) {
                            services.ConfigureVernuntii(features => features
                                .ConfigureGit(features => features
                                    .UseActiveBranchCaseVersioningMode()));
                        } else {
                            services.ConfigureVernuntii(features => features
                                .AddSemanticVersionCalculation(features => features
                                    .UseVersioningMode(overrideVersioningMode.Value)));
                        }

                        if (overridePostPreRelease != null) {
                            services.ConfigureVernuntii(features => features
                                .ConfigureGit(features => features
                                    .ConfigurePreRelease(configurer => configurer
                                        .SetPostPreRelease(overridePostPreRelease))));
                        }
                    });

                    var repository = gitServiceProvider.GetRequiredService<IRepository>();
                    var presentationFoundationProvider = calculationServiceProvider.GetRequiredService<SemanticVersionFoundationProvider>();

                    var presentationFoundation = presentationFoundationProvider.GetFoundation(
                        cacheId,
                        creationRetentionTime: cacheCreationRetentionTime,
                        lastAccessRetentionTime: cacheLastAccessRetentionTime);

                    var formattedVersion = new SemanticVersionPresentationStringBuilder(presentationFoundation)
                        .UsePresentationKind(presentationKind)
                        .UsePresentationPart(presentationParts)
                        .UsePresentationView(presentationView)
                        .BuildString();

                    var elapsedTime = stopwatch.Elapsed;
                    logger.LogInformation("Loaded version in {LoadTime}", $"{elapsedTime.ToString("s\\.f", CultureInfo.InvariantCulture)}s");

                    System.Console.WriteLine(formattedVersion);

                    if (duplicateVersionFails && repository.HasCommitVersion(presentationFoundation.Version)) {
                        return (int)ExitCode.VersionDuplicate;
                    }

                    return (int)ExitCode.Success;
                } catch (Exception error) {
                    logger.LogCritical(error, "A fatal exception occured.");
                    return (int)ExitCode.Failure;
                }
            });

        _ = new CommandLineBuilder(rootCommand)
            // .UseVersionOption() // produces exception
            .UseHelp()
            .UseEnvironmentVariableDirective()
            .UseParseDirective()
            .UseSuggestDirective()
            .RegisterWithDotnetSuggest()
            .UseTypoCorrections()
            .UseParseErrorReporting()
            //.UseExceptionHandler() // not needed
            .CancelOnProcessTermination()
            .Build();

        configureCommand?.Invoke(rootCommand);
        // This approach enables getting exit code correctly.
        return await rootCommand.InvokeAsync(args);

        void SetupLogger(LogEventLevel? verbosity, out ILoggerFactory loggerFactory, out Action<ILoggingBuilder> addSerilog)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(verbosity ?? LogEventLevel.Fatal)
                .WriteTo.Console(
                    outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    // We want to ouput log to STDERROR
                    standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();

            var addSerilogPtr = addSerilog = builder => builder.AddSerilog(logger);
            loggerFactory = LoggerFactory.Create(builder => addSerilogPtr(builder));
        }
    }

    /// <summary>
    /// Entry point of console application.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>exit code</returns>
    public static Task<int> Main(string[] args) => RunAsync(args);
}
