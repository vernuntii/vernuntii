using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.VersionIncrementFlows;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A compendium for all kind of presets.
    /// </summary>
    public interface IVersioningPresetManager
    {
        /// <summary>
        /// Registry for versioning presets.
        /// </summary>
        IVersioningPresetRegistry VersioningPresets { get; }

        /// <summary>
        /// Registry for version increment flows.
        /// </summary>
        IVersionIncrementFlowRegistry IncrementFlows { get; }

        /// <summary>
        /// Registry for message conventions.
        /// </summary>
        IMessageConventionRegistry MessageConventions { get; }

        /// <summary>
        /// Registry for message indicators.
        /// </summary>
        IMessageIndicatorRegistry MessageIndicators { get; }

        /// <summary>
        /// Registry for message indicators each crafted from a configuration.
        /// </summary>
        IConfiguredMessageIndicatorFactoryRegistry ConfiguredMessageIndicatorFactories { get; }

        /// <summary>
        /// Registry for height convention.
        /// </summary>
        IHeightConventionRegistry HeightConventions { get; }

        /// <summary>
        /// Adds a preset.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        /// <param name="mappings"></param>
        void Add(string name, IVersioningPreset preset, VersioningPresetMap mappings = VersioningPresetMap.Everything);

        /// <summary>
        /// Clears all versioning presets, message conventions, message indicators and height conventions.
        /// </summary>
        void Clear();
    }
}
