using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for registering <see cref="IVersionIncrementer"/> to <see cref="IServiceCollection"/>.
    /// </summary>
    public static class VernuntiiServicesViewExtensions
    {
        /// <summary>
        /// Adds semantic version calculator with dependencies.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiServicesView AddVersionIncrementer(this IVernuntiiServicesView view, Action<IVersionIncrementerServicesView>? configure = null)
        {
            var services = view.Services;

            if (configure is not null) {
                configure(new VersionIncrementerServicesView(services));
            }

            services.TryAddSingleton<IVersionIncrementer, VersionIncrementer>();
            return view;
        }

        /// <summary>
        /// Adds semantic version calculation with dependencies.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiServicesView AddVersionIncrementation(this IVernuntiiServicesView view, Action<IVersionIncrementationServicesView>? configure = null)
        {
            var services = view.Services;
            view.AddVersionIncrementer();

            if (configure is not null) {
                var dependencies = new VersionIncrementationServicesView(services);
                configure(dependencies);
            }

            services.TryAddScoped(sp => sp.GetRequiredService<IOptionsSnapshot<VersionIncrementationOptions>>().Value);
            services.TryAddScoped<IVersionIncrementation, VersionIncrementation>();
            return view;
        }
    }
}
