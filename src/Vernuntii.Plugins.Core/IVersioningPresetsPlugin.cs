using Vernuntii.VersioningPresets;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin to manage versioning presets.
    /// </summary>
    public interface IVersioningPresetsPlugin : IPlugin
    {
        /// <summary>
        /// Manages the presets and others.
        /// </summary>
        IVersioningPresetManager PresetManager { get; }
    }
}
