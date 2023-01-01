namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesView"/>.
    /// </summary>
    public static class BranchCasesFeaturesServicesExtensions
    {
        /// <summary>
        /// Configures an instance of <see cref="IBranchCasesServicesView"/>.
        /// </summary>
        /// <param name="view"aram>
        /// <param name="configure"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IGitServicesView ConfigureBranchCases(this IGitServicesView view, Action<IBranchCasesServicesView> configure)
        {
            var services = view.Services;

            if (configure is null) {
                throw new ArgumentNullException(nameof(configure));
            }

            var options = new BranchCasesFeatures(services);
            configure(options);
            return view;
        }
    }
}
