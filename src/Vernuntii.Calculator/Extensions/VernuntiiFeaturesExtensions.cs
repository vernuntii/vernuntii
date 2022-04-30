using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Teronis;
using Vernuntii.Extensions.VersionFoundation;
using Vernuntii.VersionFoundation;
using Vernuntii.VersionFoundation.Caching;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for registering <see cref="ISemanticVersionCalculator"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiFeaturesExtensions
    {
        /// <summary>
        /// Adds semantic version calculator with dependencies.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureFeatures"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiFeatures AddSemanticVersionCalculator(this IVernuntiiFeatures features, Action<ISemanticVersionCalculatorFeatures>? configureFeatures = null)
        {
            var services = features.Services;

            if (configureFeatures is not null) {
                configureFeatures(new SemanticVersionCalculatorFeatures(services));
            }

            services.TryAddSingleton<ISemanticVersionCalculator, SemanticVersionCalculator>();
            return features;
        }

        /// <summary>
        /// Adds semantic version calculation with dependencies.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureFeatures"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiFeatures AddSemanticVersionCalculation(this IVernuntiiFeatures features, Action<ISemanticVersionCalculationFeatures>? configureFeatures = null)
        {
            var services = features.Services;

            if (configureFeatures is not null) {
                var dependencies = new SemanticVersionCalculationFeatures(services);
                configureFeatures(dependencies);
            }

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SemanticVersionCalculationOptions>>().Value);
            services.TryAddSingleton<ISemanticVersionCalculation, SemanticVersionCalculation>();
            return features;
        }

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
