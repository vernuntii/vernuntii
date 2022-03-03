using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Autofac
{
    /// <summary>
    /// Extension methods for <see cref="ILifetimeScopedServiceProvider"/>.
    /// </summary>
    public static class LifetimeScopedServiceProviderExtensions
    {
        /// <summary>
        /// Creates a scoped lifetime.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="configureServices">Configures additional service registrations.</param>
        public static ILifetimeScopedServiceProvider CreateScope(
            this ILifetimeScopedServiceProvider serviceProvider,
            Action<IServiceCollection> configureServices)
        {
            var lifetime = serviceProvider.LifetimeScope.BeginLifetimeScope(builder => {
                var services = new ServiceCollection();
                configureServices(services);
                builder.Populate(services);
            });

            return new LifetimeScopedServiceProvider(lifetime);
        }
    }
}
