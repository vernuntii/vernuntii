namespace Vernuntii.Git
{
    /// <summary>
    /// The options for <see cref="CommitVersionFinder"/>
    /// </summary>
    public sealed class CommitVersionFinderOptions
    {
        /// <summary>
        /// Pre-release matcher.
        /// </summary>
        public ICommitVersionPreReleaseMatcher? PreReleaseMatcher { get; set; }
    }
}
