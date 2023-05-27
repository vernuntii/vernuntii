using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Plugins.Events;
using Vernuntii.Plugins.VersionPersistence;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;
using Vernuntii.VersionPersistence;

namespace Vernuntii.Plugins
{
    internal class VersionCacheOptionsPlugin : Plugin
    {
        private readonly Option<string> _cacheIdOption = new(new string[] { "--cache-id" }) {
            Description = "The non-case-sensitive cache id is used to cache the version informations once and load them on next accesses." +
                $" If cache id is not specified it is implicitly the internal cache id: {VersionCacheOptions.DefaultInternalCacheId}"
        };

        private readonly Option<TimeSpan?> _cacheCreationRetentionTimeOption = new(new string[] { "--cache-creation-retention-time" }, parseArgument: result => {
            if (result.Tokens.Count == 0 || result.Tokens[0].Value == string.Empty) {
                return null; // = internal default is taken
            }

            return TimeSpan.Parse(result.Tokens[0].Value, CultureInfo.InvariantCulture);
        }) {
            Arity = ArgumentArity.ZeroOrOne
        };

        private readonly Option<TimeSpan?> _cacheLastAccessRetentionTimeOption = new(new string[] { "--cache-last-access-retention-time" }, parseArgument: result => {
            if (result.Tokens.Count == 0 || result.Tokens[0].Value == string.Empty) {
                return null; // = feature won't be used
            }

            return TimeSpan.Parse(result.Tokens[0].Value, CultureInfo.InvariantCulture);
        }) {
            Arity = ArgumentArity.ZeroOrOne
        };
        private readonly ICommandLinePlugin _commandLinePlugin;

        public VersionCacheOptionsPlugin(ICommandLinePlugin commandLinePlugin) =>
            _commandLinePlugin = commandLinePlugin ?? throw new ArgumentNullException(nameof(commandLinePlugin));

        private Task OnParseCommandLineArguments(ParseResult parseResult)
        {
            var cacheOptions = new VersionCacheOptions();

            cacheOptions.CacheId = parseResult.GetValueForOption(_cacheIdOption);

            var cacheCreationRetentionTime = parseResult.GetValueForOption(_cacheCreationRetentionTimeOption);

            if (cacheCreationRetentionTime.HasValue) {
                cacheOptions.CacheCreationRetentionTime = cacheCreationRetentionTime.Value;
            }

            cacheOptions.LastAccessRetentionTime = parseResult.GetValueForOption(_cacheLastAccessRetentionTimeOption);
            return Events.EmitAsync(VersionCacheOptionsEvents.OnParsedVersionCacheOptions, cacheOptions);
        }

        protected override void OnExecution()
        {
            _cacheCreationRetentionTimeOption.Description = "The cache retention time since creation. If the time span since creation is greater than then" +
                " the at creation specified retention time then the version informations is reloaded. Null or empty means the" +
                $" default creation retention time of {VersionCacheOptions.DefaultCacheCreationRetentionTime.TotalHours}" +
                " hours is used.";

            _cacheLastAccessRetentionTimeOption.Description = "The cache retention time since last access. If the time span since last access is greater than the" +
                " retention time then the version informations is reloaded. Null or empty means this feature is disabled except" +
                $" if the cache id is implictly or explictly equals to the internal cache id, then the default last access retention time of" +
                $" {VersionCacheOptions.DefaultInternalCacheLastAccessRetentionTime.ToString("s\\.f", CultureInfo.InvariantCulture)}s" +
                " is used.";

            _commandLinePlugin.RootCommand.Add(_cacheCreationRetentionTimeOption);
            _commandLinePlugin.RootCommand.Add(_cacheLastAccessRetentionTimeOption);
            _commandLinePlugin.RootCommand.Add(_cacheIdOption);

            Events.OneTime(CommandLineEvents.ParsedCommandLineArguments)
                .Subscribe(OnParseCommandLineArguments);

            Events.OneTime(ServicesEvents.OnConfigureServices)
                .And(VersionCacheOptionsEvents.OnParsedVersionCacheOptions)
                .Subscribe(result => {
                    var (services, cacheOptions) = result;
                    services.AddSingleton<IVersionCacheOptions>(cacheOptions);
                });
        }
    }
}
