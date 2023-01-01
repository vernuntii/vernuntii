using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitServicesView"/>
    /// </summary>
    public static class GitServicesViewExtensions
    {
        private static BranchCase CreateBranchCase(IConfiguration configuration, Action<IBranchCase>? configureBranchCase)
        {
            var branchCaseArguments = new BranchCase();
            configuration.Bind(branchCaseArguments);
            branchCaseArguments.Extensions[BranchCase.ConfigurationExtensionName] = configuration;
            configureBranchCase?.Invoke(branchCaseArguments);
            return branchCaseArguments;
        }

        /// <summary>
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="branchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesView AddBranchCases(
            this IGitServicesView view,
            IEnumerable<IConfiguration> branchCaseSections,
            Action<IBranchCase>? configureBranchCase = null)
        {
            view.Services.TryAddSingleton<IConfiguredVersionIncrementFlowFactory, ConfiguredVersionIncrementFlowFactory>();
            view.Services.TryAddSingleton<IConfiguredMessageConventionFactory, ConfiguredMessageConventionFactory>();
            view.Services.TryAddSingleton<IConfiguredVersioningPresetFactory, ConfiguredVersioningPresetFactory>();
            return view.AddBranchCases(branchCaseSections.Select(configuration => CreateBranchCase(configuration, configureBranchCase)));
        }

        /// <summary>
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="branchCaseSection"></param>
        /// <param name="addtionalBranchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitServicesView AddBranchCases(
            this IGitServicesView view,
            IConfiguration branchCaseSection,
            IEnumerable<IConfiguration> addtionalBranchCaseSections,
            Action<IBranchCase>? configureBranchCase = null)
        {
            static IEnumerable<IConfiguration> GetConfigurations(IConfiguration configuration, IEnumerable<IConfiguration> addtionalSections)
            {
                yield return configuration;

                foreach (var additionalSection in addtionalSections) {
                    yield return additionalSection;
                }
            }

            return view.AddBranchCases(GetConfigurations(branchCaseSection, addtionalBranchCaseSections), configureBranchCase);
        }

        /// <summary>
        /// Creates versioning mode extension for each branch case and
        /// configures <see cref="VersionIncrementationOptions"/> to set
        /// <see cref="VersionIncrementationOptions.VersioningPreset"/>
        /// from active branch case.
        /// </summary>
        /// <param name="view"></param>
        public static IGitServicesView UseActiveBranchCaseVersioningMode(this IGitServicesView view)
        {
            view.ConfigureBranchCases(branchCases => branchCases.TryCreateVersioningPresetExtension());

            view.Services.AddOptions<VersionIncrementationOptions>()
                .Configure<IBranchCasesProvider>((options, branchCaseProvider) =>
                    options.VersioningPreset = branchCaseProvider.ActiveBranchCase.GetVersioningPresetExtension());

            return view;
        }
    }
}
