using Vernuntii.SemVer;

namespace Vernuntii.VersionPersistence;

public static class VersionCacheParts
{
    /// <summary>
    /// Major part.
    /// </summary>
    public static readonly VersionCachePartWithType<uint> Major = VersionCachePartWithType.FromCallerMember<uint>();

    /// <summary>
    /// Minor part.
    /// </summary>
    public static readonly VersionCachePartWithType<uint> Minor = VersionCachePartWithType.FromCallerMember<uint>();

    /// <summary>
    /// Patch part.
    /// </summary>
    public static readonly VersionCachePartWithType<uint> Patch = VersionCachePartWithType.FromCallerMember<uint>();

    /// <summary>
    /// Version core part.
    /// </summary>
    public static readonly VersionCachePartWithType<string> VersionCore = VersionCachePartWithType.FromCallerMember<string>();

    /// <summary>
    /// Pre-release part.
    /// </summary>
    public static readonly VersionCachePartWithType<string> PreRelease = VersionCachePartWithType.FromCallerMember<string>();

    /// <summary>
    /// Pre-release part with leading hyphen.
    /// </summary>
    public static readonly VersionCachePartWithType<string> HyphenPreRelease = VersionCachePartWithType.FromCallerMember<string>();

    /// <summary>
    /// Build part.
    /// </summary>
    public static readonly VersionCachePartWithType<string> Build = VersionCachePartWithType.FromCallerMember<string>();

    /// <summary>
    /// Build part with leading plus.
    /// </summary>
    public static readonly VersionCachePartWithType<string> PlusBuild = VersionCachePartWithType.FromCallerMember<string>();

    /// <summary>
    /// Semantic version.
    /// </summary>
    public static readonly VersionCachePartWithType<string> SemanticVersion = VersionCachePartWithType.FromCallerMember<string>();

    /// <summary>
    /// Semantic version.
    /// </summary>
    public static readonly VersionCachePartWithType<ISemanticVersion> Version = VersionCachePartWithType.FromCallerMember<ISemanticVersion>();

    /// <summary>
    /// Semantic version.
    /// </summary>
    public static readonly VersionCachePartWithType<ExpirationTime> ExpirationTime = VersionCachePartWithType.FromCallerMember<ExpirationTime>();

    /// <summary>
    /// Semantic version.
    /// </summary>
    public static readonly VersionCachePartWithType<DateTime?> LastAccessTime = VersionCachePartWithType.FromCallerMember<DateTime?>();
}
