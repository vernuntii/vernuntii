namespace Vernuntii.Git
{
    /// <summary>
    /// Owns the capabilities to access commits.
    /// </summary>
    public interface ICommitsAccessor
    {
        /// <summary>
        /// Gets the commits.
        /// </summary>
        /// <param name="branchName">Branch to read from</param>
        /// <param name="sinceCommit">Commit to begin from</param>
        /// <param name="fromOldToNew">If true, commits are queried from oldest to newest commit.</param>
        /// <returns>The commits.</returns>
        IEnumerable<ICommit> GetCommits(string? branchName = null, string? sinceCommit = null, bool fromOldToNew = false);
    }
}
