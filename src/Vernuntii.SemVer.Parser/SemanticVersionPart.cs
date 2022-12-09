namespace Vernuntii.SemVer.Parser;

/// <summary>
/// The parts of a semantic version.
/// </summary>
[Flags]
public enum SemanticVersionPart
{
    /// <summary>
    /// No version part.
    /// </summary>
    None = 0,
    /// <summary>
    /// The prefix.
    /// </summary>
    Prefix = 1,
    /// <summary>
    /// Any part of the version core.
    /// </summary>
    Version = 2,
    /// <summary>
    /// The major version.
    /// </summary>
    Major = Version | 4,
    /// <summary>
    /// The minor version.
    /// </summary>
    Minor = Version | 8,
    /// <summary>
    /// The patch version.
    /// </summary>
    Patch = Version | 16,
    /// <summary>
    /// The pre-release.
    /// </summary>
    PreRelease = 64,
    /// <summary>
    /// The build.
    /// </summary>
    Build = 128,
}
