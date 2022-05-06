namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Versioning preset extension.
    /// </summary>
    public sealed record class VersioningPresetExtension : IEquatable<VersioningPresetExtension>, IVersioningPresetExtension
    {
        /// <summary>
        /// The extension name.
        /// </summary>
        internal const string ExtensionName = nameof(VersioningPresetExtension);

        /// <inheritdoc/>
        public IVersioningPreset VersioningPreset {
            get => _versioningPreset ??= new VersioningPreset();
            init => _versioningPreset = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IVersioningPreset? _versioningPreset;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public VersioningPresetExtension() { }

        /// <summary>
        /// Creates a shallow copy of <paramref name="options"/>.
        /// </summary>
        /// <param name="options"></param>
        public VersioningPresetExtension(IVersioningPreset options) =>
            VersioningPreset = options;

        /// <inheritdoc/>
        public bool Equals(IVersioningPresetExtension? other) =>
            VersioningPreset.Equals(other?.VersioningPreset);

        /// <inheritdoc/>
        public bool Equals(VersioningPresetExtension? other) =>
            Equals((IVersioningPresetExtension?)other);

        /// <inheritdoc/>
        public override int GetHashCode() => VersioningPreset.GetHashCode();
    }
}
