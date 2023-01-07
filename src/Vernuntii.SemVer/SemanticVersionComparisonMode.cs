namespace Vernuntii.SemVer;

/// <summary>
/// The comparison mode for <see cref="SemanticVersionComparer"/>.
/// </summary>
public enum SemanticVersionComparisonMode
{
    /// <summary>
    /// Compares only the version numbers.
    /// </summary>
    Version,

    /// <summary>
    /// Compares version numbers and pre-release
    /// </summary>
    VersionRelease,

    /// <summary>
    /// Compares version number, pre-release and build
    /// </summary>
    VersionReleaseBuild
}
