
namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Versioning preset extension.
    /// </summary>
    public interface IVersioningPresetExtension : IEquatable<IVersioningPresetExtension>
    {
        /// <summary>
        /// The versioning preset.
        /// </summary>
        IVersioningPreset VersioningPreset { get; }
    }
}
