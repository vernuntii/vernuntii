using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesServicesView"/>.
    /// </summary>
    public static class BranchCasesServicesViewExtensions
    {
        /// <summary>
        /// Adds versioning mode extension for every branch case.
        /// </summary>
        /// <param name="view"></param>
        public static IBranchCasesServicesView TryCreateVersioningPresetExtension(this IBranchCasesServicesView view)
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
