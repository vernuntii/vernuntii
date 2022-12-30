using System.Diagnostics.CodeAnalysis;
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
        public const string DefaultMessageConventionKey = "MessageConvention";

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
        /// <param name="versionPart"></param>
        /// <param name="indicators"></param>
        /// <returns><see langword="true"/> if message indicators has been created</returns>
        protected bool TryCreateIndicators(IConfigurationSection indicatorsSection, VersionPart versionPart, out IReadOnlyCollection<IMessageIndicator>? indicators)
        {
            if (indicatorsSection.NotExisting()) {
                indicators = null;
                return false;
            }

            if (indicatorsSection.Value(out var sectionValue)) {
                indicators = new[] { PresetManager.MessageIndicators.GetItem(sectionValue) };
                return true;
            }

            var indicatorList = new List<IMessageIndicator>();

            foreach (var indicatorSection in indicatorsSection.GetChildren()) {
                string? indicatorName;

                if (!indicatorSection.Value(out indicatorName)) {
                    var indicatorObject = indicatorSection.Get<MessageIndicatorObject>();
                    indicatorName = indicatorObject?.Name;
                }

                if (string.IsNullOrEmpty(indicatorName)) {
                    throw new ConfigurationValidationException("Message indicator name is not specified");
                }

                if (PresetManager.MessageIndicators.ContainsName(indicatorName)) {
                    indicatorList.Add(PresetManager.MessageIndicators.GetItem(indicatorName));
                } else {
                    if (!PresetManager.ConfiguredMessageIndicatorFactories.ContainsName(indicatorName)) {
                        // message indicator nor factory exists
                        throw new ConfigurationValidationException($"The message indicator \"{indicatorName}\" does not exist");
                    }

                    var configuredMessageIndicatorFactory = PresetManager.ConfiguredMessageIndicatorFactories.GetItem(indicatorName);
                    indicatorList.Add(configuredMessageIndicatorFactory.Create(indicatorSection, indicatorName, versionPart));
                }
            }

            indicators = indicatorList;
            return true;
        }

        /// <inheritdoc/>
        public bool TryCreate(IDefaultConfigurationSectionProvider sectionProvider, [NotNullWhen(true)] out IMessageConvention? messageConvention)
        {
            var messageConventionSection = sectionProvider.GetSection();

            if (messageConventionSection.NotExisting()) {
                messageConvention = null;
                return false;
            }

            if (messageConventionSection.Value(out var sectionValue)) {
                messageConvention = PresetManager.MessageConventions.GetItem(sectionValue ?? nameof(InbuiltMessageConvention.Default));
                return true;
            }

            var messageConventionObject = new MessageConventionObject();
            messageConventionSection.Bind(messageConventionObject);

            IMessageConvention baseMessageConvention;

            if (messageConventionObject.Base != null) {
                baseMessageConvention = PresetManager.MessageConventions.GetItem(messageConventionObject.Base);
            } else {
                baseMessageConvention = MessageConvention.None;
            }

            var havingMajorIndicators = TryCreateIndicators(messageConventionSection.GetSection(DefaultMajorIndicatorsKey), VersionPart.Major, out var majorIndicators);
            var havingMinorIndicators = TryCreateIndicators(messageConventionSection.GetSection(DefaultMinorIndicatorsKey), VersionPart.Minor, out var minorIndicators);
            var havingPatchIndicators = TryCreateIndicators(messageConventionSection.GetSection(DefaultPatchIndicatorsKey), VersionPart.Patch, out var patchIndicators);

            if (majorIndicators is null
                && minorIndicators is null
                && patchIndicators is null) {
                messageConvention = baseMessageConvention;
                return true;
            }

            messageConvention = new MessageConvention(baseMessageConvention) with {
                MajorIndicators = havingMajorIndicators ? majorIndicators : baseMessageConvention.MajorIndicators,
                MinorIndicators = havingMinorIndicators ? minorIndicators : baseMessageConvention.MinorIndicators,
                PatchIndicators = havingPatchIndicators ? patchIndicators : baseMessageConvention.PatchIndicators
            };

            return true;
        }

        internal class MessageConventionObject
        {
            public string? Base { get; set; }
        }

        internal class MessageIndicatorObject
        {
            public string? Name { get; set; }
        }
    }
}
