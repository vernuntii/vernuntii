using Vernuntii.VersioningPresets;

namespace Vernuntii.Git;

internal class VersionIncrementerTestSuiteOptions
{
    public required IVersioningPreset VersioningPreset { get; init; }
    public string? SearchPreRelease { get; init; }
    public string? PostPreRelease { get; init; }
}
