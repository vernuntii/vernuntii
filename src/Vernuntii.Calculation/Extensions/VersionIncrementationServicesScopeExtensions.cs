using Microsoft.Extensions.DependencyInjection;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IVersionIncrementation"/>.
    /// </summary>
    public static class VersionIncrementationServicesScopeExtensions
    {
        /// <summary>
        /// Configures <see cref="VersionIncrementationOptions"/> to set
        /// <see cref="VersionIncrementationOptions.VersioningPreset"/>
        /// from <paramref name="versioningModePreset"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="versioningModePreset"></param>
        public static IVersionIncrementationServicesScope UseVersioningMode(this IVersionIncrementationServicesScope features, string versioningModePreset)
        {
            features.Services.AddOptions<VersionIncrementationOptions>()
                .Configure<IVersioningPresetRegistry>((options, registry) =>
                    options.VersioningPreset = registry.GetItem(versioningModePreset));

            return features;
        }
    }
}
