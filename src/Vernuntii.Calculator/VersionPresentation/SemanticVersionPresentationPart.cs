namespace Vernuntii.VersionPresentation
{
    /// <summary>
    /// Parts of semantic version presentation.
    /// </summary>
    [Flags]
    public enum SemanticVersionPresentationPart
    {
        /// <summary>
        /// Major part.
        /// </summary>
        Major = 1,
        /// <summary>
        /// Minor part.
        /// </summary>
        Minor = 2,
        /// <summary>
        /// Patch part.
        /// </summary>
        Patch = 4,
        /// <summary>
        /// Version core part.
        /// </summary>
        Version = 8,
        /// <summary>
        /// Pre-release part.
        /// </summary>
        PreRelease = 16,
        /// <summary>
        /// Pre-release part with leading hyphen.
        /// </summary>
        HyphenPreRelease = 32,
        /// <summary>
        /// Build part.
        /// </summary>
        Build = 64,
        /// <summary>
        /// Build part with leading plus.
        /// </summary>
        PlusBuild = 128,
        /// <summary>
        /// Semantic version.
        /// </summary>
        SemanticVersion = 256,
        /// <summary>
        /// Branch name part.
        /// </summary>
        BranchName = 512,
        /// <summary>
        /// Commit sha part.
        /// </summary>
        CommitSha = 1024,
        /// <summary>
        /// All presentation parts.
        /// </summary>
        All = Major | Minor | Patch | Version | PreRelease | HyphenPreRelease | Build | PlusBuild | SemanticVersion | BranchName | CommitSha
    }
}
