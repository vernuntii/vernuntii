using Vernuntii.VersioningPresets;

namespace Vernuntii.Extensions.BranchCases
{
    public static class BranchCasesFeaturesExtensions
    {
        public static IBranchCasesFeatures TryCreateVersioningModeExtension(this IBranchCasesFeatures features)
        {
            features.Services.ConfigureVernuntii(features => features
                .ConfigureGit(features => features
                    .ConfigureBranchCases<IVersioningPresetCompendium>((branchCase, presetCompendium) => branchCase.TryCreateVersioningModeExtension(presetCompendium))));

            return features;
        }
    }
}
