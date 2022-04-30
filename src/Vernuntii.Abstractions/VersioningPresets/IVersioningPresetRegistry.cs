namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A registry for presets.
    /// </summary>
    public interface IVersioningPresetRegistry
    {
        /// <summary>
        /// Data source of versioning presets.
        /// </summary>
        IReadOnlyDictionary<string, IVersioningPreset> VersioningPresets { get; }

        /// <summary>
        /// Adds a preset associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        void AddPreset(string name, IVersioningPreset preset);

        /// <summary>
        /// Clears all versioning presets.
        /// </summary>
        void ClearVersioningPresets();
    }
}
