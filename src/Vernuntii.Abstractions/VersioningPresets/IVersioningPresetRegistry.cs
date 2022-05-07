namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A registry for presets.
    /// </summary>
    public interface IVersioningPresetRegistry
    {
        /// <summary>
        /// Versioning preset identifiers.
        /// </summary>
        IEnumerable<string> VersioningPresetIdentifiers { get; }

        /// <summary>
        /// Adds a preset associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        void AddVersioningPreset(string name, IVersioningPreset preset);

        /// <summary>
        /// Gets preset by name.
        /// </summary>
        /// <param name="name"></param>
        IVersioningPreset GetVersioningPreset(string name);

        /// <summary>
        /// Clears all versioning presets.
        /// </summary>
        void ClearVersioningPresets();
    }
}
