namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesScope"/>.
    /// </summary>
    public static class GitFeaturesServicesExtensions
    {
        /// <summary>
        /// Configures an instance of <see cref="IGitServicesScope"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiServicesScope ScopeToGit(this IVernuntiiServicesScope features, Action<IGitServicesScope> configure)
        {
            var services = features.Services;

            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new GitServicesScope(services);
            configure(options);
            return features;
        }

        /// <summary>
        /// Configures an instance of <see cref="IGitServicesScope"/>.
        /// </summary>
        /// <param name="features"></param>
        public static IGitServicesScope ScopeToGit(this IVernuntiiServicesScope features) =>
            new GitServicesScope(features.Services);
    }
}
