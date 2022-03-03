using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Vernuntii.Extensions.VersionFoundation;
using Vernuntii.VersionFoundation;
using Vernuntii.VersionFoundation.Caching;
using Teronis;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IVernuntiiFeatures"/>.
    /// </summary>
    public static class VernuntiiFeaturesExtensions
    {
        internal static IVernuntiiFeatures AddSemanticVersionFoundationCache(this IVernuntiiFeatures features)
        {
            var services = features.Services;
            services.AddOptions();
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SemanticVersionFoundationCacheOptions>>().Value);
            services.TryAddSingleton<SemanticVersionFoundationCache<SemanticVersionFoundation>>();
            return features;
        }

        /// <summary>
        /// Adds <see cref="SemanticVersionFoundationProvider"/> to services.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureOptions"></param>
        public static IVernuntiiFeatures AddSemanticVersionFoundationProvider(
            this IVernuntiiFeatures features,
            Action<SemanticVersionFoundationProviderOptions>? configureOptions = null)
        {
            features.AddSemanticVersionFoundationCache();
            var services = features.Services;
            var optionsBuilder = services.AddOptions<SemanticVersionFoundationProviderOptions>();
            services.AddLogging();

            if (configureOptions != null) {
                optionsBuilder.Configure(configureOptions);
            }

            services.TryAddSingleton(sp => new SlimLazy<ISemanticVersionCalculation>(() => sp.GetRequiredService<ISemanticVersionCalculation>()));
            services.TryAddSingleton<ISemanticVersionFoundationCache<SemanticVersionFoundation>, SemanticVersionFoundationCache<SemanticVersionFoundation>>();
            services.TryAddSingleton<SemanticVersionFoundationProvider>();

            return features;
        }
    }
}
