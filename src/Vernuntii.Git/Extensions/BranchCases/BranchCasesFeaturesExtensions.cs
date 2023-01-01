using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesServicesView"/>.
    /// </summary>
    public static class BranchCasesFeaturesExtensions
    {
        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesServicesView ForEach(this IBranchCasesServicesView view, Action<IBranchCase> configureBranchCase)
        {
            view.Services.AddOptions<BranchCasesOptions>()
                .Configure(options => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments);
                    }
                });

            return view;
        }

        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesServicesView ForEach<TDependency>(this IBranchCasesServicesView view, Action<IBranchCase, TDependency> configureBranchCase)
            where TDependency : class
        {
            view.Services.AddOptions<BranchCasesOptions>()
                .Configure<TDependency>((options, dependency) => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments, dependency);
                    }
                });

            return view;
        }

        /// <summary>
        /// Configures instances of <see cref="IBranchCase"/> of <see cref="BranchCasesOptions"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="configureBranchCase"></param>
        public static IBranchCasesServicesView ForEach<TDependency1, TDependency2>(this IBranchCasesServicesView view, Action<IBranchCase, TDependency1, TDependency2> configureBranchCase)
            where TDependency1 : class
            where TDependency2 : class
        {
            view.Services.AddOptions<BranchCasesOptions>()
                .Configure<TDependency1, TDependency2>((options, dependency, dependency2) => {
                    foreach (var branchCaseArguments in options.BranchCases.Values) {
                        configureBranchCase(branchCaseArguments, dependency, dependency2);
                    }
                });

            return view;
        }
    }
}
