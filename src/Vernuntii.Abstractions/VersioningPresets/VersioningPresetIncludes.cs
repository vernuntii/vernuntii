namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Indicating what preset does include.
    /// </summary>
    public enum VersioningPresetIncludes
    {
        /// <summary>
        /// Preset includes message convention.
        /// </summary>
        MessageConvention = 1,
        /// <summary>
        /// Preset includes height convention.
        /// </summary>
        HeightConvention = 2,
        /// <summary>
        /// Preset includes everything.
        /// </summary>
        Preset = MessageConvention | HeightConvention
    }
}
