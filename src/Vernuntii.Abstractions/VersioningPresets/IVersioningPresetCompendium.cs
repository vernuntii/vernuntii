namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A compendium for all kind of presets.
    /// </summary>
    public interface IVersioningPresetManager : IVersioningPresetRegistry, IMessageConventionRegistry, IHeightConventionRegistry, IMessageIndicatorRegistry
    {
        /// <summary>
        /// Adds a preset.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        /// <param name="includes"></param>
        void Add(string name, IVersioningPreset preset, VersioningPresetIncludes includes = VersioningPresetIncludes.Preset);

        /// <summary>
        /// Clears all versioning presets, message conventions and height conventions.
        /// </summary>
        void Clear();
    }
}
