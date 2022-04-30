using Microsoft.Extensions.Configuration;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.VersioningPresets;

namespace Vernuntii.MessageVersioning
{
    /// <summary>
    /// Extension options for <see cref="VersionIncrementBuilder"/>
    /// </summary>
    public sealed record class MessageVersioningModeExtensionOptions : IEquatable<MessageVersioningModeExtensionOptions>
    {
        /// <summary>
        /// The "VersioningMode"-key.
        /// </summary>
        public const string VersioningModeKey = "VersioningMode";

        /// <summary>
        /// A factory for <see cref="MessageVersioningModeExtensionOptions"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="presetCompendium"></param>
        /// <exception cref="ArgumentException"></exception>
        public static Func<MessageVersioningModeExtensionOptions> CreateFactory(IConfiguration configuration, IVersioningPresetCompendium presetCompendium)
        {
            var versioningModeSection = configuration.GetSection(VersioningModeKey);
            Func<MessageVersioningModeExtensionOptions> extensionFactory;

            if (!versioningModeSection.Exists() || versioningModeSection.Value != null) {
                extensionFactory = CreateFromString;

                MessageVersioningModeExtensionOptions CreateFromString() => new MessageVersioningModeExtensionOptions {
                    VersioningPreset = presetCompendium.GetVersioningPreset(configuration.GetValue(VersioningModeKey, default(string)))
                };
            } else {
                extensionFactory = CreateFromObject;

                MessageVersioningModeExtensionOptions CreateFromObject()
                {
                    var versioningModeObject = new VersioningModeObject();
                    versioningModeSection.Bind(versioningModeObject);

                    if (versioningModeObject.MessageConvention == null && versioningModeObject.Preset == null) {
                        throw new ArgumentException($"Field \"{nameof(versioningModeObject.MessageConvention)}\" is null." +
                            $" Either set it or specifiy the field \"{nameof(versioningModeObject.Preset)}\".");
                    } else if (versioningModeObject.IncrementMode == null && versioningModeObject.Preset == null) {
                        throw new ArgumentException($"Field \"{nameof(versioningModeObject.IncrementMode)}\" is null." +
                            $" Either set it or specifiy the field \"{nameof(versioningModeObject.Preset)}\".");
                    }

                    IVersioningPreset basePreset;

                    if (versioningModeObject.Preset != null) {
                        basePreset = presetCompendium.GetVersioningPreset(versioningModeObject.Preset);
                    } else {
                        basePreset = presetCompendium.GetVersioningPreset(VersioningPresetKind.Default);
                    }

                    IMessageConvention? messageConvention;

                    if (versioningModeObject.MessageConvention != null) {
                        messageConvention = presetCompendium.GetMessageConvention(versioningModeObject.MessageConvention);
                    } else {
                        messageConvention = basePreset.MessageConvention;
                    }

                    IHeightConvention? heightConvention = basePreset.HeightConvention;
                    var versionIncrementMode = versioningModeObject.IncrementMode ?? basePreset.IncrementMode;

                    return new MessageVersioningModeExtensionOptions(new VersioningPreset() {
                        IncrementMode = versionIncrementMode,
                        MessageConvention = messageConvention,
                        HeightConvention = heightConvention
                    });
                }
            }

            return extensionFactory;
        }

        /// <summary>
        /// The extension name.
        /// </summary>
        internal const string ExtensionName = nameof(MessageVersioningModeExtensionOptions);

        /// <summary>
        /// Represents the version core options for <see cref="VersionIncrementBuilder"/>.
        /// </summary>
        public IVersioningPreset VersioningPreset {
            get => _versioningPreset ??= new VersioningPreset();
            init => _versioningPreset = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IVersioningPreset? _versioningPreset;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageVersioningModeExtensionOptions() { }

        /// <summary>
        /// Creates a shallow copy of <paramref name="options"/>.
        /// </summary>
        /// <param name="options"></param>
        public MessageVersioningModeExtensionOptions(IVersioningPreset options) =>
            VersioningPreset = options;

        /// <inheritdoc/>
        public bool Equals(MessageVersioningModeExtensionOptions? other) =>
            VersioningPreset.Equals(other?.VersioningPreset);

        /// <inheritdoc/>
        public override int GetHashCode() => VersioningPreset.GetHashCode();

        internal class VersioningModeObject
        {
            public VersionIncrementMode? IncrementMode { get; set; }
            public string? Preset { get; set; }
            public string? MessageConvention { get; set; }
        }
    }
}
