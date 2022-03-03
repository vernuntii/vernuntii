using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    public static class GitFeaturesExtensions
    {
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
