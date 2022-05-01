using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Vernuntii.MessageVersioning;
using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin that contains shared options for <see cref="INextVersionPlugin"/>
    /// and <see cref="IGitPlugin"/>.
    /// </summary>
    public class NextVersionOptionsPlugin : Plugin
    {
        /// <summary>
        /// Override versioning mode.
        /// </summary>
        public string? OverrideVersioningMode { get; set; }

        /// <summary>
        /// True of <see cref="OverrideVersioningMode"/> is not <see langword="null"/>.
        /// </summary>
        [MemberNotNullWhen(true, nameof(OverrideVersioningMode))]
        public bool ShouldOverrideVersioningMode => OverrideVersioningMode != null;

        private Option<string?> _overrideVersioningModeOption = null!;

        /// <inheritdoc/>
        protected override void OnCompletedRegistration()
        {
            var versioningPresetsPlugin = FirstPlugin<IVersioningPresetsPlugin>();

            _overrideVersioningModeOption = new Option<string?>(new[] { "--override-versioning-mode" })
                .AddCompletions(_ => versioningPresetsPlugin.PresetManager.VersioningPresets.Keys);

            FirstPlugin<ICommandLinePlugin>().RootCommand.Add(_overrideVersioningModeOption);
        }

        /// <inheritdoc/>
        protected override void OnEvents() =>
            Events.SubscribeOnce(CommandLineEvents.ParsedCommandLineArgs, parseResult =>
                OverrideVersioningMode = parseResult.GetValueForOption(_overrideVersioningModeOption));
    }
}
