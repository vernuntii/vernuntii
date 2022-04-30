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
            registry.VersioningPresets[versioningPresetName ?? nameof(VersioningPresetKind.Default)];

        /// <summary>
        /// Gets versioning preset.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="versioningPresetKind"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IVersioningPreset GetVersioningPreset(this IVersioningPresetRegistry registry, VersioningPresetKind? versioningPresetKind) =>
            registry.GetVersioningPreset(versioningPresetKind?.ToString());
    }
}
