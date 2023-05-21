using Vernuntii.VersioningPresets;

namespace Vernuntii.Git;

internal class VersionIncrementerTestSuiteOptions
{
    public IVersioningPreset VersioningPreset { get; }
    public string? SearchPreRelease { get; init; }
    public string? PostPreRelease { get; init; }

    public VersionIncrementerTestSuiteOptions(IVersioningPreset versioningPreset) =>
        VersioningPreset = versioningPreset;
}
