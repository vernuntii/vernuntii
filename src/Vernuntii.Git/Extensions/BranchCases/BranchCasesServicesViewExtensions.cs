using Microsoft.Extensions.DependencyInjection;
using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesServicesView"/>.
    /// </summary>
    public static class BranchCasesServicesViewExtensions
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
                    foreach (var branchCase in options.BranchCases.Values) {
                        configureBranchCase(branchCase, dependency);
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
                    foreach (var branchCase in options.BranchCases.Values) {
                        configureBranchCase(branchCase, dependency, dependency2);
                    }
                });

            return view;
        }

        /// <summary>
        /// Adds versioning mode extension for every branch case.
        /// </summary>
        /// <param name="view"></param>
        public static IBranchCasesServicesView ForEachSetVersioningPresetExtensionFactory(this IBranchCasesServicesView view)
        {
            view.Services
                .TakeViewOfVernuntii()
                .TakeViewOfGit()
                .ConfigureBranchCases(branchCases => branchCases
                    .ForEach<IConfiguredVersioningPresetFactory>((branchCase, presetFactory) => branchCase.SetVersioningPresetExtensionFactory(presetFactory)));

            return view;
        }
    }
}
