using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesServicesScope"/>.
    /// </summary>
    public static class BranchCasesFeaturesExtensions
    {
        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesServicesScope ForEach(this IBranchCasesServicesScope features, Action<IBranchCase> configureBranchCase)
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
        public static IBranchCasesServicesScope ForEach<TDependency>(this IBranchCasesServicesScope features, Action<IBranchCase, TDependency> configureBranchCase)
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

        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesServicesScope ForEach<TDependency1, TDependency2>(this IBranchCasesServicesScope features, Action<IBranchCase, TDependency1, TDependency2> configureBranchCase)
            where TDependency1 : class
            where TDependency2 : class
        {
            features.Services.AddOptions<BranchCasesOptions>()
                .Configure<TDependency1, TDependency2>((options, dependency, dependency2) => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments, dependency, dependency2);
                    }
                });

            return features;
        }
    }
}
