namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Indicating what versioning preset does map.
    /// </summary>
    public enum VersioningPresetMappings
    {
        /// <summary>
        /// Preset maps itself.
        /// </summary>
        VersioningPreset = 1,
        /// <summary>
        /// Preset maps message convention.
        /// </summary>
        MessageConvention = 2,
        /// <summary>
        /// Preset maps height convention.
        /// </summary>
        HeightConvention = 4,
        /// <summary>
        /// Preset maps everything.
        /// </summary>
        Everything = VersioningPreset | MessageConvention | HeightConvention
    }
}
