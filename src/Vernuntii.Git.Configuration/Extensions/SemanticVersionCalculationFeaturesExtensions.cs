using Microsoft.Extensions.DependencyInjection;
using Vernuntii.MessageVersioning;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ISemanticVersionCalculation"/>.
    /// </summary>
    public static class SemanticVersionCalculationFeaturesExtensions
    {
        /// <summary>
        /// Configures <see cref="SemanticVersionCalculationOptions"/> to set
        /// <see cref="SemanticVersionCalculationOptions.VersioningPreset"/>
        /// from <paramref name="versioningModePreset"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="versioningModePreset"></param>
        public static ISemanticVersionCalculationFeatures UseVersioningMode(this ISemanticVersionCalculationFeatures features, string versioningModePreset)
        {
            features.Services.AddOptions<SemanticVersionCalculationOptions>()
                .Configure<IVersioningPresetRegistry>((options, registry) =>
                    options.VersioningPreset = registry.GetVersioningPreset(versioningModePreset));

            return features;
        }
    }
}
