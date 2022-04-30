using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesFeatures"/>.
    /// </summary>
    public static class BranchCasesFeaturesExtensions
    {
        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesFeatures ForEach(this IBranchCasesFeatures features, Action<IBranchCase> configureBranchCase)
        {
            features.Services.AddOptions<BranchCasesOptions>()
                .Configure(options => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments);
                    }
                });

            return features;
        }

        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesFeatures ForEach<TDependency>(this IBranchCasesFeatures features, Action<IBranchCase, TDependency> configureBranchCase)
            where TDependency : class
        {
            features.Services.AddOptions<BranchCasesOptions>()
                .Configure<TDependency>((options, dependency) => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments, dependency);
                    }
                });

            return features;
        }
    }
}
