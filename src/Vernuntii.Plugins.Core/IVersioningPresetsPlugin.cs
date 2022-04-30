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
        IVersioningPresetManager PresetManager { get; }
    }
}
