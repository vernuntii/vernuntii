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
        /// <param name="reverse">If true commits are queried in reverse order</param>
        /// <returns>The commits.</returns>
        IEnumerable<ICommit> GetCommits(string? branchName = null, string? sinceCommit = null, bool reverse = false);
    }
}
