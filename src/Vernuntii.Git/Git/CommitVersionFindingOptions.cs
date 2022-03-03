namespace Vernuntii.Git
{
    /// <summary>
    /// The options for <see cref="ICommitVersionFinder.FindCommitVersion(CommitVersionFindingOptions)"/>
    /// </summary>
    public sealed class CommitVersionFindingOptions
    {
        /// <summary>
        /// The since-commit where to start reading from.
        /// </summary>
        public string? SinceCommit { get; set; }
        /// <summary>
        /// The branch reading the commits from.
        /// </summary>
        public string? BranchName { get; set; }
        /// <summary>
        /// If null or empty non-pre-release versions are included in
        /// search.
        /// If specified then all non-release AND version with this
        /// pre-release are included in search.
        /// </summary>
        public string? PreRelease { get; set; }
    }
}
