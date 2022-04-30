namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitFeatures"/>.
    /// </summary>
    public static class BranchCasesFeaturesServicesExtensions
    {
        /// <summary>
        /// Configures an instance of <see cref="IBranchCasesFeatures"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureFeatures"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IGitFeatures ConfigureBranchCases(this IGitFeatures features, Action<IBranchCasesFeatures> configureFeatures)
        {
            var services = features.Services;

            if (configureFeatures is null) {
                throw new ArgumentNullException(nameof(configureFeatures));
            }

            var options = new BranchCasesFeatures(services);
            configureFeatures(options);
            return features;
        }
    }
}
