namespace Vernuntii.SemVer;

/// <summary>
/// The available formats to present the version.
/// </summary>
[Flags]
public enum SemanticVersionFormat
{
    /// <summary>
    /// The prefix.
    /// </summary>
    Prefix = 1,
    /// <summary>
    /// The version core.
    /// </summary>
    VersionCore = 32,
    /// <summary>
    /// The pre-release.
    /// </summary>
    PreRelease = 64,
    /// <summary>
    /// The build.
    /// </summary>
    Build = 128,
    /// <summary>
    /// The version core and pre-release.
    /// </summary>
    VersionRelease = VersionCore | PreRelease,
    /// <summary>
    /// The version core, pre-release and build.
    /// </summary>
    VersionReleaseBuild = VersionRelease | Build,
    /// <summary>
    /// The full version representation.
    /// </summary>
    SemanticVersion = Prefix | VersionCore | PreRelease | Build,
}
