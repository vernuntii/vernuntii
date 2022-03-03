using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
    }
}
