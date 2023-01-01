using Microsoft.Extensions.DependencyInjection;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IVersionIncrementation"/>.
    /// </summary>
    public static class VersionIncrementationServicesViewExtensions
    {
        /// <summary>
        /// Configures <see cref="VersionIncrementationOptions"/> to set
        /// <see cref="VersionIncrementationOptions.VersioningPreset"/>
        /// from <paramref name="versioningModePreset"/>.
        /// </summary>
        /// <param name="view"aram>
        /// <param name="versioningModePreset"></param>
        public static IVersionIncrementationServicesView UseVersioningMode(this IVersionIncrementationServicesView view, string versioningModePreset)
        {
            view.Services.AddOptions<VersionIncrementationOptions>()
                .Configure<IVersioningPresetRegistry>((options, registry) =>
                    options.VersioningPreset = registry.GetItem(versioningModePreset));

            return view;
        }
    }
}
