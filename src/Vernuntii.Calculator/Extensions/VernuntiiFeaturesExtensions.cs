using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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

            if (configureFeatures is not null) {
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

            if (configureFeatures is not null) {
                var dependencies = new SingleVersionCalculationFeatures(services);
                configureFeatures(dependencies);
            }

            services.TryAddSingleton(sp => sp.GetRequiredService<IOptions<SingleVersionCalculationOptions>>().Value);
            services.TryAddSingleton<ISingleVersionCalculation, SingleVersionCalculation>();
            return features;
        }
    }
}
