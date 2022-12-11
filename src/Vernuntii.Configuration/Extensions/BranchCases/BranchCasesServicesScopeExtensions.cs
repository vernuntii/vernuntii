using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesServicesScope"/>.
    /// </summary>
    public static class BranchCasesServicesScopeExtensions
    {
        /// <summary>
        /// Adds versioning mode extension for every branch case.
        /// </summary>
        /// <param name="scope"></param>
        public static IBranchCasesServicesScope TryCreateVersioningPresetExtension(this IBranchCasesServicesScope scope)
        {
            scope.Services.ScopeToVernuntii(vernuntii => vernuntii
                .ScopeToGit(features => features
                    .ConfigureBranchCases(branchCases => branchCases
                        .ForEach<IConfiguredVersioningPresetFactory>((branchCase, presetFactory) => branchCase.SetVersioningPresetExtensionFactory(presetFactory)))));

            return scope;
        }
    }
}
