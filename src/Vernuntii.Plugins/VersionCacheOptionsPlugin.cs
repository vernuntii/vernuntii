using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersionCaching;

namespace Vernuntii.Plugins
{
    internal class VersionCacheOptionsPlugin : Plugin
    {
        public VersionCacheOptions CacheOptions { get; set; } = new VersionCacheOptions();

        private Option<string> _cacheIdOption = new Option<string>(new string[] { "--cache-id" }) {
            Description = "The non-case-sensitive cache id is used to cache the version informations once and load them on next accesses." +
                $" If cache id is not specified it is implicitly the internal cache id: {VersionCacheOptions.DefaultInternalCacheId}"
        };

        private Option<TimeSpan?> _cacheCreationRetentionTimeOption = new Option<TimeSpan?>(new string[] { "--cache-creation-retention-time" }, parseArgument: result => {
            if (result.Tokens.Count == 0 || result.Tokens[0].Value == string.Empty) {
                return null; // = internal default is taken
            }

            return TimeSpan.Parse(result.Tokens[0].Value, CultureInfo.InvariantCulture);
        }) {
            Arity = ArgumentArity.ZeroOrOne
        };

        private Option<TimeSpan?> _cacheLastAccessRetentionTimeOption = new Option<TimeSpan?>(new string[] { "--cache-last-access-retention-time" }, parseArgument: result => {
            if (result.Tokens.Count == 0 || result.Tokens[0].Value == string.Empty) {
                return null; // = feature won't be used
            }

            return TimeSpan.Parse(result.Tokens[0].Value, CultureInfo.InvariantCulture);
        }) {
            Arity = ArgumentArity.ZeroOrOne
        };

        /// <inheritdoc/>
        protected override void OnAfterRegistration()
        {
            var commandLinePlugin = Plugins.First<ICommandLinePlugin>();

            _cacheCreationRetentionTimeOption.Description = "The cache retention time since creation. If the time span since creation is greater than then" +
                " the at creation specified retention time then the version informations is reloaded. Null or empty means the" +
                $" default creation retention time of {CacheOptions.CacheCreationRetentionTime.TotalHours}" +
                " hours is used.";

            _cacheLastAccessRetentionTimeOption.Description = "The cache retention time since last access. If the time span since last access is greater than the" +
            " retention time then the version informations is reloaded. Null or empty means this feature is disabled except" +
            $" if the cache id is implictly or explictly equals to the internal cache id, then the default last access retention time of" +
            $" {CacheOptions.InternalCacheLastAccessRetentionTime.ToString("s\\.f", CultureInfo.InvariantCulture)}s" +
            " is used.";

            commandLinePlugin.RootCommand.Add(_cacheCreationRetentionTimeOption);
            commandLinePlugin.RootCommand.Add(_cacheLastAccessRetentionTimeOption);
            commandLinePlugin.RootCommand.Add(_cacheIdOption);
        }

        private void OnParseCommandLineArgs(ParseResult parseResult)
        {
            CacheOptions.CacheId = parseResult.GetValueForOption(_cacheIdOption);

            var cacheCreationRetentionTime = parseResult.GetValueForOption(_cacheCreationRetentionTimeOption);

            if (cacheCreationRetentionTime.HasValue) {
                CacheOptions.CacheCreationRetentionTime = cacheCreationRetentionTime.Value;
            }

            CacheOptions.LastAccessRetentionTime = parseResult.GetValueForOption(_cacheLastAccessRetentionTimeOption);
        }

        protected override void OnEvents()
        {
            Events.SubscribeOnce(
                CommandLineEvents.ParsedCommandLineArgs,
                OnParseCommandLineArgs);

            Events.SubscribeOnce(
                GlobalServicesEvents.ConfigureServices,
                services => services.AddSingleton<IVersionCacheOptions>(CacheOptions));
        }
    }
}
