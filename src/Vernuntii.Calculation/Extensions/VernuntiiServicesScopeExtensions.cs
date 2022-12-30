using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for registering <see cref="IVersionIncrementer"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiServicesScopeExtensions
    {
        /// <summary>
        /// Adds semantic version calculator with dependencies.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiServicesScope AddVersionIncrementer(this IVernuntiiServicesScope scope, Action<IVersionIncrementerServicesScope>? configure = null)
        {
            var services = scope.Services;

            if (configure is not null) {
                configure(new VersionIncrementerServicesScope(services));
            }

            services.TryAddSingleton<IVersionIncrementer, VersionIncrementer>();
            return scope;
        }

        /// <summary>
        /// Adds semantic version calculation with dependencies.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiServicesScope AddVersionIncrementation(this IVernuntiiServicesScope scope, Action<IVersionIncrementationServicesScope>? configure = null)
        {
            var services = scope.Services;
            scope.AddVersionIncrementer();

            if (configure is not null) {
                var dependencies = new VersionIncrementationServicesScope(services);
                configure(dependencies);
            }

            services.TryAddScoped(sp => sp.GetRequiredService<IOptionsSnapshot<VersionIncrementationOptions>>().Value);
            services.TryAddScoped<IVersionIncrementation, VersionIncrementation>();
            return scope;
        }
    }
}
