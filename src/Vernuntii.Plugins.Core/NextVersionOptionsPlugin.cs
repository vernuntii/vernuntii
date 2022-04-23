using System.CommandLine;
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
        public VersioningModePreset? OverrideVersioningMode { get; set; }

        private Option<VersioningModePreset?> _overrideVersioningModeOption = new Option<VersioningModePreset?>(new[] { "--override-versioning-mode" });

        /// <inheritdoc/>
        protected override void OnCompletedRegistration() =>
            PluginRegistry.First<ICommandLinePlugin>().Registered += plugin => plugin.RootCommand.Add(_overrideVersioningModeOption);

        /// <inheritdoc/>
        protected override void OnEventAggregator() =>
            SubscribeEvent(CommandLineEvents.ParsedCommandLineArgs.Discriminator, parseResult =>
                OverrideVersioningMode = parseResult.GetValueForOption(_overrideVersioningModeOption));
    }
}
