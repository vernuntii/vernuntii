using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiServicesViewExtensions
    {
        /// <summary>
        /// Configures "messages versioning"-features.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        public static IServiceCollection TakeViewOfVernuntii(this IServiceCollection services, Action<IVernuntiiServicesView>? configure)
        {
            configure?.Invoke(new VernuntiiFeatures(services));
            return services;
        }

        /// <summary>
        /// Configures "messages versioning"-features.
        /// </summary>
        /// <param name="services"></param>
        public static IVernuntiiServicesView TakeViewOfVernuntii(this IServiceCollection services) =>
            new VernuntiiFeatures(services);

        private class VernuntiiFeatures : IVernuntiiServicesView
        {
            public IServiceCollection Services { get; set; }

            public VernuntiiFeatures(IServiceCollection services) =>
                Services = services;
        }
    }
}
