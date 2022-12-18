using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiServicesScopeExtensions
    {
        /// <summary>
        /// Configures "messages versioning"-features.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        public static IServiceCollection ScopeToVernuntii(this IServiceCollection services, Action<IVernuntiiServicesScope>? configure)
        {
            configure?.Invoke(new VernuntiiFeatures(services));
            return services;
        }

        /// <summary>
        /// Configures "messages versioning"-features.
        /// </summary>
        /// <param name="services"></param>
        public static IVernuntiiServicesScope ScopeToVernuntii(this IServiceCollection services) =>
            new VernuntiiFeatures(services);

        private class VernuntiiFeatures : IVernuntiiServicesScope
        {
            public IServiceCollection Services { get; set; }

            public VernuntiiFeatures(IServiceCollection services) =>
                Services = services;
        }
    }
}
