using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IGitFeatures"/>
    /// </summary>
    public static class GitFeaturesExtensions
    {
        private static BranchCaseArguments CreateBranchCaseArguments(IConfiguration configuration, Action<IBranchCaseArguments>? configureBranchCase)
        {
            var branchCaseArguments = new BranchCaseArguments();
            configuration.Bind(branchCaseArguments);
            branchCaseArguments.Extensions[BranchCaseArguments.ConfigurationExtensionName] = configuration;
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
            Action<IBranchCaseArguments>? configureBranchCase = null) =>
            features.AddBranchCases(branchCaseSections.Select(configuration => CreateBranchCaseArguments(configuration, configureBranchCase)));

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
            Action<IBranchCaseArguments>? configureBranchCase = null)
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
        /// configures <see cref="SemanticVersionCalculationOptions"/> to set
        /// <see cref="SemanticVersionCalculationOptions.VersionCoreOptions"/>
        /// from active branch case.
        /// </summary>
        /// <param name="features"></param>
        public static IGitFeatures UseActiveBranchCaseVersioningMode(this IGitFeatures features)
        {
            features.ConfigureBranchCases(branchCase => branchCase.TryCreateVersioningModeExtension());

            features.Services.AddOptions<SemanticVersionCalculationOptions>()
                .Configure<IBranchCaseArgumentsProvider>((options, provider) =>
                    options.VersionCoreOptions = provider.ActiveBranchCase.GetVersioningModeExtension().VersionTransformerOptions);

            return features;
        }
    }
}
