using Autofac;
using Autofac.Extensions.DependencyInjection;
using Vernuntii.Autofac;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <inheritdoc/>
        public static LifetimeScopedServiceProvider BuildLifetimeScopedServiceProvider(this IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            builder.Populate(services);
            return new LifetimeScopedServiceProvider(builder.Build());
        }
    }
}
