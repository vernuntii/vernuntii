using Microsoft.Extensions.DependencyInjection;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ISingleVersionCalculation"/>.
    /// </summary>
    public static class SemanticVersionCalculationFeaturesExtensions
    {
        /// <summary>
        /// Configures <see cref="SingleVersionCalculationOptions"/> to set
        /// <see cref="SingleVersionCalculationOptions.VersioningPreset"/>
        /// from <paramref name="versioningModePreset"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="versioningModePreset"></param>
        public static ISingleVersionCalculationFeatures UseVersioningMode(this ISingleVersionCalculationFeatures features, string versioningModePreset)
        {
            features.Services.AddOptions<SingleVersionCalculationOptions>()
                .Configure<IVersioningPresetRegistry>((options, registry) =>
                    options.VersioningPreset = registry.GetItem(versioningModePreset));

            return features;
        }
    }
}
