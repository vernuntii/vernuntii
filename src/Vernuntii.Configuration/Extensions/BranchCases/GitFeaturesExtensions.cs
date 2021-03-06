using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitFeatures"/>
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private static BranchCase CreateBranchCaseArguments(IConfiguration configuration, Action<IBranchCase>? configureBranchCase)
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
        /// <param name="features"></param>
        /// <param name="branchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitFeatures AddBranchCases(
            this IGitFeatures features,
            IEnumerable<IConfiguration> branchCaseSections,
            Action<IBranchCase>? configureBranchCase = null)
        {
            features.Services.TryAddSingleton<IConfiguredVersionIncrementFlowFactory, ConfiguredVersionIncrementFlowFactory>();
            features.Services.TryAddSingleton<IConfiguredMessageConventionFactory, ConfiguredMessageConventionFactory>();
            features.Services.TryAddSingleton<IConfiguredVersioningPresetFactory, ConfiguredVersioningPresetFactory>();
            return features.AddBranchCases(branchCaseSections.Select(configuration => CreateBranchCaseArguments(configuration, configureBranchCase)));
        }

        /// <summary>
        /// Adds multiple branch cases from configuration sections.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="branchCaseSection"></param>
        /// <param name="addtionalBranchCaseSections"></param>
        /// <param name="configureBranchCase"></param>
        public static IGitFeatures AddBranchCases(
            this IGitFeatures features,
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

            return features.AddBranchCases(GetConfigurations(branchCaseSection, addtionalBranchCaseSections), configureBranchCase);
        }

        /// <summary>
        /// Creates versioning mode extension for each branch case and
        /// configures <see cref="SingleVersionCalculationOptions"/> to set
        /// <see cref="SingleVersionCalculationOptions.VersioningPreset"/>
        /// from active branch case.
        /// </summary>
        /// <param name="features"></param>
        public static IGitFeatures UseActiveBranchCaseVersioningMode(this IGitFeatures features)
        {
            features.ConfigureBranchCases(branchCases => branchCases.TryCreateVersioningPresetExtension());

            features.Services.AddOptions<SingleVersionCalculationOptions>()
                .Configure<IBranchCasesProvider>((options, branchCaseProvider) =>
                    options.VersioningPreset = branchCaseProvider.ActiveBranchCase.GetVersioningPresetExtension());

            return features;
        }
    }
}
