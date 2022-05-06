using Microsoft.Extensions.Configuration;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A factory for <see cref="VersioningPresetExtension"/>.
    /// </summary>
    public class VersioningPresetExtensionFactory : IVersioningPresetExtensionFactory
    {
        /// <summary>
        /// The "VersioningMode"-key.
        /// </summary>
        public const string VersioningModeKey = "VersioningMode";

        private readonly IVersioningPresetManager _presetManager;

        /// <summary>
        /// Creates an type of this type.
        /// </summary>
        /// <param name="presetManager"></param>
        public VersioningPresetExtensionFactory(IVersioningPresetManager presetManager) =>
            _presetManager = presetManager;

        /// <inheritdoc/>
        public IVersioningPresetExtension Create(IConfiguration branchConfiguration)
        {
            var versioningModeSection = branchConfiguration.GetSection(VersioningModeKey);

            // 'VersioningMode:' also results into empty string.
            if (!versioningModeSection.Exists() || versioningModeSection.Value != null) {
                var versioningMode = branchConfiguration.GetValue(VersioningModeKey, nameof(InbuiltVersioningPreset.Default));

                return new VersioningPresetExtension {
                    VersioningPreset = _presetManager.GetVersioningPreset(versioningMode)
                };
            }

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
                basePreset = _presetManager.GetVersioningPreset(versioningModeObject.Preset);
            } else {
                basePreset = _presetManager.GetVersioningPreset(InbuiltVersioningPreset.Manual);
            }

            IMessageConvention? messageConvention;

            if (versioningModeObject.MessageConvention != null) {
                messageConvention = _presetManager.GetMessageConvention(versioningModeObject.MessageConvention);
            } else {
                messageConvention = basePreset.MessageConvention;
            }

            IHeightConvention? heightConvention = basePreset.HeightConvention;
            var versionIncrementMode = versioningModeObject.IncrementMode ?? basePreset.IncrementMode;
            var rightShiftWhenZeroMajor = versioningModeObject.RightShiftWhenZeroMajor;

            return new VersioningPresetExtension(new VersioningPreset() {
                IncrementMode = versionIncrementMode,
                MessageConvention = messageConvention,
                HeightConvention = heightConvention,
                RightShiftWhenZeroMajor = rightShiftWhenZeroMajor
            });
        }

        internal class VersioningModeObject
        {
            public VersionIncrementMode? IncrementMode { get; set; }
            public string? Preset { get; set; }
            public string? MessageConvention { get; set; }
            public bool RightShiftWhenZeroMajor { get; set; }
        }
    }
}
