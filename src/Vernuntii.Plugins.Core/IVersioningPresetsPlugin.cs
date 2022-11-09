using Vernuntii.PluginSystem.Lifecycle;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Plugins
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
