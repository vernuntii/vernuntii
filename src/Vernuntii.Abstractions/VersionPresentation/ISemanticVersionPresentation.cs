namespace Vernuntii.VersionPresentation
{
    /// <summary>
    /// The representation of a version.
    /// </summary>
    public interface ISemanticVersionPresentation
    {
        /// <summary>
        /// Major version number.
        /// </summary>
        uint? Major { get; }
        /// <summary>
        /// Minor version number.
        /// </summary>
        uint? Minor { get; }
        /// <summary>
        /// Patch version number.
        /// </summary>
        uint? Patch { get; }
        /// <summary>
        /// E.g. 0.1.0
        /// </summary>
        string? Version { get; }
        /// <summary>
        /// E.g. alpha
        /// </summary>
        string? PreRelease { get; }
        /// <summary>
        /// E.g. -alpha
        /// </summary>
        string? HyphenPreRelease { get; }
        /// <summary>
        /// E.g. 3
        /// </summary>
        string? Build { get; }
        /// <summary>
        /// E.g. +3
        /// </summary>
        string? PlusBuild { get; }
        /// <summary>
        /// 0.1.0-alpha+3
        /// </summary>
        string? SemanticVersion { get; }
        /// <summary>
        /// The current branch name.
        /// </summary>
        string? BranchName { get; }
        /// <summary>
        /// The commit sha of the current branch.
        /// </summary>
        string? CommitSha { get; }
    }
}
