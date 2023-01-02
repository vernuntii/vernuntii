namespace Vernuntii.VersionPersistence;

/// <summary>
/// Represents a version presentation part.
/// </summary>
public sealed class GitVersionCacheParts
{
    public static readonly VersionCachePartWithType<string> BranchName = VersionCachePartWithType.FromCallerMember<string>();
    public static readonly VersionCachePartWithType<string> BranchTip = VersionCachePartWithType.FromCallerMember<string>();
}
