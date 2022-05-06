using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Extension methods for <see cref="IVersioningPresetRegistry"/>.
    /// </summary>
    public static class VersioningPresetRegistryExtensions
    {
        /// <summary>
        /// Gets versioning preset.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="versioningPresetName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IVersioningPreset GetVersioningPreset(this IVersioningPresetRegistry registry, string? versioningPresetName) =>
            registry.VersioningPresets[versioningPresetName ?? nameof(InbuiltVersioningPreset.Default)];

        /// <summary>
        /// Gets versioning preset.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="InbuiltVersioningPreset"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IVersioningPreset GetVersioningPreset(this IVersioningPresetRegistry registry, InbuiltVersioningPreset? InbuiltVersioningPreset) =>
            registry.GetVersioningPreset(InbuiltVersioningPreset?.ToString());
    }
}
