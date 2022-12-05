﻿using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Vernuntii.Configuration.Json;
using Vernuntii.Configuration.Yaml;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Meta;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// A plugin that contains shared options.
    /// </summary>
    [Plugin(Order = -1500)]
    public class SharedOptionsPlugin : Plugin
    {
        /// <summary>
        /// Override versioning mode.
        /// </summary>
        public string? OverrideVersioningMode {
            get {
                EnsureParsedCommandLineArgs();
                return _overrideVersioningMode;
            }

            set => _overrideVersioningMode = value;
        }

        /// <summary>
        /// The Vernuntii configuration file path.
        /// If the config path is null the current
        /// working directory is returned.
        /// The config path is available after 
        /// </summary>
        [AllowNull]
        public string ConfigPath {
            get {
                EnsureParsedCommandLineArgs();
                return _configPath ?? Directory.GetCurrentDirectory();
            }

            set => _configPath = value;
        }

        /// <summary>
        /// True of <see cref="OverrideVersioningMode"/> is not <see langword="null"/>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(OverrideVersioningMode))]
        public bool ShouldOverrideVersioningMode => OverrideVersioningMode != null;

        private bool _areCommandLineArgsParsed;
        private Option<string?> _overrideVersioningModeOption = null!;

        private readonly Option<string?> _configPathOption = new(new[] { "--config-path", "-c" }) {
            Description = $"The configuration file path. JSON and YAML is allowed. If a directory is specified instead the configuration file" +
                $" {YamlConfigurationFileDefaults.YmlFileName}, {YamlConfigurationFileDefaults.YamlFileName} or {JsonConfigurationFileDefaults.JsonFileName}" +
                " (in each upward directory in this exact order) is searched at specified directory and above."
        };

        private readonly IVersioningPresetsPlugin _versioningPresetsPlugin;
        private readonly ICommandLinePlugin _commandLinePlugin;
        private string? _configPath;
        private string? _overrideVersioningMode;

        public SharedOptionsPlugin(IVersioningPresetsPlugin versioningPresetsPlugin, ICommandLinePlugin commandLinePlugin)
        {
            _versioningPresetsPlugin = versioningPresetsPlugin ?? throw new ArgumentNullException(nameof(versioningPresetsPlugin));
            _commandLinePlugin = commandLinePlugin ?? throw new ArgumentNullException(nameof(commandLinePlugin));
        }

        private void EnsureParsedCommandLineArgs()
        {
            if (!_areCommandLineArgsParsed) {
                throw new InvalidOperationException("Command-line args are not yet parsed");
            }
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            _overrideVersioningModeOption = new Option<string?>(new[] { "--override-versioning-mode" })
                .AddCompletions(_ => _versioningPresetsPlugin.PresetManager.VersioningPresets.Names);

            _commandLinePlugin.RootCommand.Add(_overrideVersioningModeOption);
            _commandLinePlugin.RootCommand.Add(_configPathOption);

            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, parseResult => {
                Events.Publish(SharedOptionsEvents.ParseCommandLineArgs);

                // Parse options.
                _overrideVersioningMode = parseResult.GetValueForOption(_overrideVersioningModeOption);
                _configPath = parseResult.GetValueForOption(_configPathOption);

                _areCommandLineArgsParsed = true;
                Events.Publish(SharedOptionsEvents.ParsedCommandLineArgs);
            });
        }
    }
}