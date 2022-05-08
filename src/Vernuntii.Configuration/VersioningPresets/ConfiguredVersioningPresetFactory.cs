using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration;
using Vernuntii.Extensions;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A factory for <see cref="IVersioningPreset"/>.
    /// </summary>
    public class ConfiguredVersioningPresetFactory : IConfiguredVersioningPresetFactory
    {
        /// <summary>
        /// The "VersioningMode"-key.
        /// </summary>
        public const string DefaultVersioningModeKey = "VersioningMode";
        /// <summary>
        /// The default preset key.
        /// </summary>
        public const string DefaultPresetKey = nameof(VersioningModeObject.Preset);

        /// <summary>
        /// The versioning preset manager containing everything related to versioning presets.
        /// </summary>
        protected IVersioningPresetManager PresetManager { get; }

        private readonly IConfiguredMessageConventionFactory _messageConventionFactory;

        /// <summary>
        /// Creates a type of this type.
        /// </summary>
        /// <param name="presetManager"></param>
        /// <param name="messageConventionFactory"></param>
        public ConfiguredVersioningPresetFactory(IVersioningPresetManager presetManager, IConfiguredMessageConventionFactory messageConventionFactory)
        {
            PresetManager = presetManager;
            _messageConventionFactory = messageConventionFactory;
        }

        /// <inheritdoc/>
        public IVersioningPreset Create(IDefaultConfigurationSectionProvider sectionProvider)
        {
            var versioningModeSection = sectionProvider.GetSection();

            if (versioningModeSection.HavingValue()) {
                return PresetManager.VersioningPresets.GetItem(versioningModeSection.Value ?? nameof(InbuiltVersioningPreset.Default));
            }

            var versioningModeObject = new VersioningModeObject();
            versioningModeSection.Bind(versioningModeObject);

            if (versioningModeObject.IncrementMode == null && versioningModeObject.Preset == null) {
                throw new ConfigurationValidationException($"Field \"{nameof(versioningModeObject.IncrementMode)}\" is null." +
                    $" Either set it or specifiy the field \"{DefaultPresetKey}\".");
            }

            IVersioningPreset basePreset;

            if (versioningModeObject.Preset != null) {
                basePreset = PresetManager.VersioningPresets.GetItem(versioningModeObject.Preset);
            } else {
                basePreset = PresetManager.VersioningPresets.GetItem(nameof(InbuiltVersioningPreset.Manual));
            }

            var havingMessageConvention = _messageConventionFactory.TryCreate(
                versioningModeSection.GetSectionProvider(ConfiguredMessageConventionFactory.DefaultMessageConventionKey), out var messageConvention);

            if (!havingMessageConvention && versioningModeObject.Preset == null) {
                throw new ConfigurationValidationException($"Field \"{ConfiguredMessageConventionFactory.DefaultMessageConventionKey}\" is null." +
                    $" Either set it or specifiy the field \"{nameof(versioningModeObject.Preset)}\".");
            }

            IHeightConvention? heightConvention = basePreset.HeightConvention;
            var versionIncrementMode = versioningModeObject.IncrementMode ?? basePreset.IncrementMode;
            var rightShiftWhenZeroMajor = versioningModeObject.RightShiftWhenZeroMajor;

            return new VersioningPreset() {
                IncrementMode = versionIncrementMode,
                MessageConvention = havingMessageConvention ? messageConvention : basePreset.MessageConvention,
                HeightConvention = heightConvention,
                RightShiftWhenZeroMajor = rightShiftWhenZeroMajor
            };
        }

        internal class VersioningModeObject
        {
            public VersionIncrementMode? IncrementMode { get; set; }
            public string? Preset { get; set; }
            public bool RightShiftWhenZeroMajor { get; set; }
        }
    }
}
