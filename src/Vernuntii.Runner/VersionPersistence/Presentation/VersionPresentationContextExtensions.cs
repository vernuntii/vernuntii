namespace Vernuntii.VersionPersistence.Presentation;

internal static class VersionPresentationContextExtensions
{
    public static void ImportNextVersionRequirements(this VersionPresentationContext context)
    {
        context.PresentableParts.Add(VersionCacheParts.Major);
        context.PresentableParts.Add(VersionCacheParts.Minor);
        context.PresentableParts.Add(VersionCacheParts.Patch);
        context.PresentableParts.Add(VersionCacheParts.VersionCore);
        context.PresentableParts.Add(VersionCacheParts.PreRelease);
        context.PresentableParts.Add(VersionCacheParts.HyphenPreRelease);
        context.PresentableParts.Add(VersionCacheParts.Build);
        context.PresentableParts.Add(VersionCacheParts.PlusBuild);
        context.PresentableParts.Add(VersionCacheParts.SemanticVersion);
    }

    public static void ImportGitRequirements(this VersionPresentationContext context)
    {
        context.PresentableParts.Add(GitVersionCacheParts.BranchName);
        context.PresentableParts.Add(GitVersionCacheParts.BranchTip);
    }
}
