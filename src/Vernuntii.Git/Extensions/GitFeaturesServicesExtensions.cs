namespace Vernuntii.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IGitFeatures"/>.
    /// </summary>
    public static class GitFeaturesServicesExtensions
    {
        /// <summary>
        /// Configures an instance of <see cref="IGitFeatures"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureFeatures"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IVernuntiiFeatures ConfigureGit(this IVernuntiiFeatures features, Action<IGitFeatures> configureFeatures)
        {
            var services = features.Services;

            if (configureFeatures is null) {
                throw new ArgumentNullException(nameof(configureFeatures));
            }

            var options = new GitFeatures(services);
            configureFeatures(options);
            return features;
        }
    }
}
