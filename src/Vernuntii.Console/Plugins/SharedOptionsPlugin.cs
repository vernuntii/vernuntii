using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Meta;
using Vernuntii.PluginSystem.Reactive;

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
                EnsureParsedCommandLineArguments();
                return _overrideVersioningMode;
            }

            set => _overrideVersioningMode = value;
        }

        // TODO:
        ///// <summary>
        ///// The Vernuntii configuration file path.
        ///// If the config path is null the current
        ///// working directory is returned.
        ///// The config path is available after 
        ///// </summary>
        //[AllowNull]
        //public string ConfigPath {
        //    get {
        //        EnsureParsedCommandLineArguments();
        //        return _configPath ?? Directory.GetCurrentDirectory();
        //    }

        //    set => _configPath = value;
        //}

        /// <summary>
        /// True of <see cref="OverrideVersioningMode"/> is not <see langword="null"/>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(OverrideVersioningMode))]
        public bool ShouldOverrideVersioningMode => OverrideVersioningMode != null;

        private bool _areCommandLineArgumentsParsed;
        private Option<string?> _overrideVersioningModeOption = null!;

        private readonly IVersioningPresetsPlugin _versioningPresetsPlugin;
        private readonly ICommandLinePlugin _commandLinePlugin;
        private string? _overrideVersioningMode;

        public SharedOptionsPlugin(IVersioningPresetsPlugin versioningPresetsPlugin, ICommandLinePlugin commandLinePlugin)
        {
            _versioningPresetsPlugin = versioningPresetsPlugin ?? throw new ArgumentNullException(nameof(versioningPresetsPlugin));
            _commandLinePlugin = commandLinePlugin ?? throw new ArgumentNullException(nameof(commandLinePlugin));
        }

        private void EnsureParsedCommandLineArguments()
        {
            if (!_areCommandLineArgumentsParsed) {
                throw new InvalidOperationException("Command-line args are not yet parsed");
            }
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            _overrideVersioningModeOption = new Option<string?>(new[] { "--override-versioning-mode" })
                .AddCompletions(_ => _versioningPresetsPlugin.PresetManager.VersioningPresets.Names);

            _commandLinePlugin.RootCommand.Add(_overrideVersioningModeOption);

            Events
                .Earliest(CommandLineEvents.ParsedCommandLineArguments)
                .Subscribe(async parseResult => {
                    await Events.FulfillAsync(SharedOptionsEvents.ParseCommandLineArguments);

                    // Parse options.
                    _overrideVersioningMode = parseResult.GetValueForOption(_overrideVersioningModeOption);

                    _areCommandLineArgumentsParsed = true;
                    await Events.FulfillAsync(SharedOptionsEvents.ParsedCommandLineArguments);
                })
                .DisposeWhenDisposing(this);
        }
    }
}
