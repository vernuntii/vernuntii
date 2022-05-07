using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration;
using Vernuntii.Extensions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.VersioningPresets;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// A factory for <see cref="IMessageConvention"/>.
    /// </summary>
    public class ConfiguredMessageConventionFactory : IConfiguredMessageConventionFactory
    {
        /// <summary>
        /// The "MessageConvention"-key.
        /// </summary>
        public const string DefaultMessageConventionKey = nameof(MessageConventionObject.MessageConvention);

        /// <summary>
        /// The default key for major indicators.
        /// </summary>
        public const string DefaultMajorIndicatorsKey = "MajorIndicators";

        /// <summary>
        /// The default key for minor indicators.
        /// </summary>
        public const string DefaultMinorIndicatorsKey = "MinorIndicators";

        /// <summary>
        /// The default key for patch indicators.
        /// </summary>
        public const string DefaultPatchIndicatorsKey = "PatchIndicators";

        /// <summary>
        /// The versioning preset manager.
        /// </summary>
        protected IVersioningPresetManager PresetManager { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="presetManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConfiguredMessageConventionFactory(IVersioningPresetManager presetManager) =>
            PresetManager = presetManager ?? throw new ArgumentNullException(nameof(presetManager));

        /// <summary>
        /// Tries to create indicators.
        /// </summary>
        /// <param name="indicatorsSection"></param>
        /// <param name="indicators"></param>
        /// <returns><see langword="true"/> if message indicators has been created</returns>
        protected bool TryCreateIndicators(IConfigurationSection indicatorsSection, out IReadOnlyCollection<IMessageIndicator>? indicators)
        {
            if (indicatorsSection.HavingValue()) {
                if (indicatorsSection.Value != null) {
                    indicators = new[] { PresetManager.GetMessageIndicator(indicatorsSection.Value) };
                    return true;
                }

                indicators = null;
                return false;
            }

            var indicatorObjectList = indicatorsSection.Get<List<MessageIndicatorObject>?>();

            if (indicatorObjectList is null) {
                indicators = null;
                return true;
            }

            var indicatorList = new List<IMessageIndicator>();

            if (indicatorObjectList.Count == 0) {
                goto exitTruthy;
            }

            foreach (var indicatorObject in indicatorObjectList) {

            }

            exitTruthy:
            indicators = indicatorList;
            return true;
        }

        /// <inheritdoc/>
        public bool TryCreate(IDefaultConfigurationSectionProvider sectionProvider, out IMessageConvention? messageConvention)
        {
            var messageConventionSection = sectionProvider.GetSection();

            if (messageConventionSection.NotExisting()) {
                messageConvention = null;
                return false;
            }

            if (messageConventionSection.HavingValue()) {
                messageConvention = PresetManager.GetMessageConvention(messageConventionSection.Value ?? nameof(InbuiltMessageConvention.Default));
                return true;
            }

            var messageConventionObject = new MessageConventionObject();
            messageConventionSection.Bind(messageConventionObject);

            IMessageConvention? baseMessageConvention;

            if (messageConventionObject.MessageConvention != null) {
                baseMessageConvention = PresetManager.GetMessageConvention(messageConventionObject.MessageConvention);
            } else {
                baseMessageConvention = null;
            }

            var havingMajorIndicators = TryCreateIndicators(messageConventionSection.GetSection(DefaultMajorIndicatorsKey), out var majorIndicators);
            var havingMinorIndicators = TryCreateIndicators(messageConventionSection.GetSection(DefaultMinorIndicatorsKey), out var minorIndicators);
            var havingPatchIndicators = TryCreateIndicators(messageConventionSection.GetSection(DefaultPatchIndicatorsKey), out var patchIndicators);

            if (majorIndicators is null
                && minorIndicators is null
                && patchIndicators is null) {
                messageConvention = baseMessageConvention;
                return true;
            }

            var emptyOrShallowMessageConvention = baseMessageConvention is null
                ? MessageConvention.Empty
                : new MessageConvention(baseMessageConvention);

            messageConvention = emptyOrShallowMessageConvention with {
                MajorIndicators = havingMajorIndicators ? majorIndicators : emptyOrShallowMessageConvention.MajorIndicators,
                MinorIndicators = havingMinorIndicators ? minorIndicators : emptyOrShallowMessageConvention.MinorIndicators,
                PatchIndicators = havingPatchIndicators ? patchIndicators : emptyOrShallowMessageConvention.PatchIndicators
            };

            return true;
        }

        internal class MessageConventionObject
        {
            public string? MessageConvention { get; set; }
        }

        internal class MessageIndicatorObject
        {
            public string? Name { get; set; }
        }
    }
}
