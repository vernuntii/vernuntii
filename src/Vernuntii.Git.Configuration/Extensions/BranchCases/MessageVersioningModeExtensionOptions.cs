using Vernuntii.MessageVersioning;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension options for <see cref="VersionTransformerBuilder"/>
    /// </summary>
    public sealed record class MessageVersioningModeExtensionOptions : IEquatable<MessageVersioningModeExtensionOptions>
    {
        /// <summary>
        /// Creates an instance of <see cref="MessageVersioningModeExtensionOptions"/> with preset.
        /// </summary>
        /// <param name="presetName"></param>
        public static MessageVersioningModeExtensionOptions WithPreset(string? presetName) => new MessageVersioningModeExtensionOptions() {
            VersionTransformerOptions = VersionTransformerBuilderOptions.GetPreset(presetName)
        };

        /// <summary>
        /// Creates an instance of <see cref="MessageVersioningModeExtensionOptions"/> with preset.
        /// </summary>
        /// <param name="preset"></param>
        public static MessageVersioningModeExtensionOptions WithPreset(VersioningModePreset preset) => new MessageVersioningModeExtensionOptions() {
            VersionTransformerOptions = VersionTransformerBuilderOptions.GetPreset(preset)
        };

        /// <summary>
        /// Represents the version core options for <see cref="VersionTransformerBuilder"/>.
        /// </summary>
        public VersionTransformerBuilderOptions VersionTransformerOptions {
            get => _versionTransformerOptions ??= new VersionTransformerBuilderOptions();
            init => _versionTransformerOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The extension name.
        /// </summary>
        internal const string ExtensionName = nameof(MessageVersioningModeExtensionOptions);
        private VersionTransformerBuilderOptions? _versionTransformerOptions;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageVersioningModeExtensionOptions() { }

        /// <summary>
        /// Creates a shallow copy of <paramref name="options"/>.
        /// </summary>
        /// <param name="options"></param>
        public MessageVersioningModeExtensionOptions(VersionTransformerBuilderOptions options) =>
            VersionTransformerOptions = options;

        /// <inheritdoc/>
        public bool Equals(MessageVersioningModeExtensionOptions? other) =>
            VersionTransformerOptions.Equals(other?.VersionTransformerOptions);

        /// <inheritdoc/>
        public override int GetHashCode() => VersionTransformerOptions.GetHashCode();
    }
}
