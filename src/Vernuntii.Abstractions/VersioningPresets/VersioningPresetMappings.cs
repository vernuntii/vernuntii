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
        /// Preset maps increment flow.
        /// </summary>
        IncrementFlow = 2,
        /// <summary>
        /// Preset maps message convention.
        /// </summary>
        MessageConvention = 4,
        /// <summary>
        /// Preset maps height convention.
        /// </summary>
        HeightConvention = 8,
        /// <summary>
        /// Preset maps everything.
        /// </summary>
        Everything = VersioningPreset | IncrementFlow | MessageConvention | HeightConvention
    }
}
