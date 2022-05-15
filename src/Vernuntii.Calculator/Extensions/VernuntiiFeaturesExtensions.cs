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
    /// Extension methods for registering <see cref="ISingleVersionCalculator"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiFeaturesExtensions
    {
        /// <summary>
        /// Adds semantic version calculator with dependencies.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureFeatures"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiFeatures AddSingleVersionCalculator(this IVernuntiiFeatures features, Action<ISingleVersionCalculatorFeatures>? configureFeatures = null)
        {
            var services = features.Services;

            if (configureFeatures is not null)
            {
                configureFeatures(new SingleVersionCalculatorFeatures(services));
            }

            services.TryAddSingleton<ISingleVersionCalculator, SingleVersionCalculator>();
            return features;
        }

        /// <summary>
        /// Adds semantic version calculation with dependencies.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureFeatures"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiFeatures AddSingleVersionCalculation(this IVernuntiiFeatures features, Action<ISingleVersionCalculationFeatures>? configureFeatures = null)
        {
            var services = features.Services;

            if (configureFeatures is not null)
            {
                var dependencies = new SingleVersionCalculationFeatures(services);
                configureFeatures(dependencies);
            }

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SingleVersionCalculationOptions>>().Value);
            services.TryAddSingleton<ISingleVersionCalculation, SingleVersionCalculation>();
            return features;
        }

        internal static IVernuntiiFeatures AddSingleVersionFoundationCache(this IVernuntiiFeatures features)
        {
            var services = features.Services;
            services.AddOptions();
            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SemanticVersionFoundationCacheOptions>>().Value);
            services.TryAddSingleton<SemanticVersionFoundationCache<SemanticVersionFoundation>>();
            return features;
        }

        /// <summary>
        /// Adds <see cref="VersionFoundationProvider"/> to services.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureOptions"></param>
        public static IVernuntiiFeatures AddSemanticVersionFoundationProvider(
            this IVernuntiiFeatures features,
            Action<VersionFoundationProviderOptions>? configureOptions = null)
        {
            features.AddSingleVersionFoundationCache();
            var services = features.Services;
            var optionsBuilder = services.AddOptions<VersionFoundationProviderOptions>();

            if (configureOptions != null)
            {
                optionsBuilder.Configure(configureOptions);
            }

            services.TryAddSingleton(sp => new SlimLazy<ISingleVersionCalculation>(() => sp.GetRequiredService<ISingleVersionCalculation>()));
            services.TryAddSingleton<ISemanticVersionFoundationCache<SemanticVersionFoundation>, SemanticVersionFoundationCache<SemanticVersionFoundation>>();
            services.TryAddSingleton<VersionFoundationProvider>();

            return features;
        }
    }
}
