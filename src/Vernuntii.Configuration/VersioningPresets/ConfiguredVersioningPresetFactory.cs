using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration;
using Vernuntii.Extensions;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersionIncrementing;

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
        private readonly IConfiguredVersionIncrementFlowFactory _incrementFlowFactory;

        /// <summary>
        /// Creates a type of this type.
        /// </summary>
        /// <param name="presetManager"></param>
        /// <param name="messageConventionFactory"></param>
        /// <param name="incrementFlowFactory"></param>
        public ConfiguredVersioningPresetFactory(
            IVersioningPresetManager presetManager,
            IConfiguredMessageConventionFactory messageConventionFactory,
            IConfiguredVersionIncrementFlowFactory incrementFlowFactory)
        {
            PresetManager = presetManager;
            _messageConventionFactory = messageConventionFactory;
            _incrementFlowFactory = incrementFlowFactory;
        }

        /// <inheritdoc/>
        public IVersioningPreset Create(IDefaultConfigurationSectionProvider sectionProvider)
        {
            var versioningModeSection = sectionProvider.GetSection();

            if (versioningModeSection.NotExistingOrValue(out var sectionValue)) {
                return PresetManager.VersioningPresets.GetItem(sectionValue ?? nameof(InbuiltVersioningPreset.Default));
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

            if (!_incrementFlowFactory.TryCreate(
                versioningModeSection.GetSectionProvider(ConfiguredVersionIncrementFlowFactory.DefaultIncrementFlowKey),
                out var incrementFlow)) {
                incrementFlow = basePreset.IncrementFlow;
            }

            if (!_messageConventionFactory.TryCreate(
                versioningModeSection.GetSectionProvider(ConfiguredMessageConventionFactory.DefaultMessageConventionKey),
                out var messageConvention)) {
                if (versioningModeObject.Preset == null) {
                    throw new ConfigurationValidationException($"Field \"{ConfiguredMessageConventionFactory.DefaultMessageConventionKey}\" is null." +
                        $" Either set it or specifiy the field \"{nameof(versioningModeObject.Preset)}\".");
                }

                messageConvention = basePreset.MessageConvention;
            }

            IHeightConvention? heightConvention = basePreset.HeightConvention;
            var versionIncrementMode = versioningModeObject.IncrementMode ?? basePreset.IncrementMode;

            return new VersioningPreset() {
                IncrementMode = versionIncrementMode,
                IncrementFlow = incrementFlow,
                MessageConvention = messageConvention,
                HeightConvention = heightConvention,
            };
        }

        internal class VersioningModeObject
        {
            public VersionIncrementMode? IncrementMode { get; set; }
            public string? Preset { get; set; }
        }
    }
}
