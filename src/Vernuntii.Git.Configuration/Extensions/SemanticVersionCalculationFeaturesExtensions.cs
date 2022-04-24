using Microsoft.Extensions.DependencyInjection;
using Vernuntii.MessageVersioning;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ISemanticVersionCalculation"/>.
    /// </summary>
    public static class SemanticVersionCalculationFeaturesExtensions
    {
        /// <summary>
        /// Configures <see cref="SemanticVersionCalculationOptions"/> to set
        /// <see cref="SemanticVersionCalculationOptions.VersionIncrementOptions"/>
        /// from <paramref name="versioningModePreset"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="versioningModePreset"></param>
        public static ISemanticVersionCalculationFeatures UseVersioningMode(this ISemanticVersionCalculationFeatures features, VersioningModePreset versioningModePreset)
        {
            features.Services.AddOptions<SemanticVersionCalculationOptions>()
                .Configure(options =>
                    options.VersionIncrementOptions = VersionIncrementBuilderOptions.GetPreset(versioningModePreset.ToString()));

            return features;
        }
    }
}
