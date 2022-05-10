using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    /// <summary>
    /// Extension methods for <see cref="IBranchCasesFeatures"/>.
    /// </summary>
    public static class BranchCasesFeaturesExtensions
    {
        /// <summary>
        /// Adds versioning mode extension for every branch case.
        /// </summary>
        /// <param name="features"></param>
        public static IBranchCasesFeatures TryCreateVersioningPresetExtension(this IBranchCasesFeatures features)
        {
            features.Services.ConfigureVernuntii(vernuntii => vernuntii
                .ConfigureGit(features => features
                    .ConfigureBranchCases(branchCases => branchCases
                        .ForEach<IConfiguredVersioningPresetFactory>((branchCase, presetFactory) => branchCase.SetVersioningPresetExtensionFactory(presetFactory)))));

            return features;
        }
    }
}
