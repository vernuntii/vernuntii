using Vernuntii.VersioningPresets;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin to manage versioning presets.
    /// </summary>
    public interface IVersioningPresetsPlugin : IPlugin
    {
        /// <summary>
        /// The compendium of presets.
        /// </summary>
        IVersioningPresetCompendium Presets { get; }
    }
}
