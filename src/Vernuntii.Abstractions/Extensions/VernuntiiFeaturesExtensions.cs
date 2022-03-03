using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiFeaturesExtensions
    {
        /// <summary>
        /// Configures "messages versioning"-features.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureFeatures"></param>
        public static IServiceCollection ConfigureVernuntii(this IServiceCollection services, Action<IVernuntiiFeatures>? configureFeatures)
        {
            configureFeatures?.Invoke(new VernuntiiFeatures(services));
            return services;
        }

        private class VernuntiiFeatures : IVernuntiiFeatures
        {
            public IServiceCollection Services { get; set; }

            public VernuntiiFeatures(IServiceCollection services) =>
                Services = services;
        }
    }
}
