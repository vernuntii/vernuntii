namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesScope"/>.
    /// </summary>
    public static class BranchCasesFeaturesServicesExtensions
    {
        /// <summary>
        /// Configures an instance of <see cref="IBranchCasesServicesScope"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IGitServicesScope ConfigureBranchCases(this IGitServicesScope features, Action<IBranchCasesServicesScope> configure)
        {
            var services = features.Services;

            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new BranchCasesFeatures(services);
            configure(options);
            return features;
        }
    }
}
