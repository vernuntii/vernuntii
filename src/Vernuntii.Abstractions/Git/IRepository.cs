namespace Vernuntii.Git
{
    /// <summary>
    /// A minimal functional repository.
    /// </summary>
    public interface IRepository : ICommitsAccessor, ICommitTagsAccessor, ICommitVersionsAccessor
    {
        /// <summary>
        /// A collection of branches.
        /// </summary>
        IBranches Branches { get; }

        /// <summary>
        /// Expands branch name to long name.
        /// </summary>
        /// <param name="branchName">Short or partial branch name.</param>
        /// <returns>Long branch name or <see langword="null"/>.</returns>
        string? ExpandBranchName(string? branchName);

        /// <summary>
        /// Gets active branch.
        /// </summary>
        IBranch GetActiveBranch();

        /// <summary>
        /// Gets the .git directory.
        /// </summary>
        string GetGitDirectory();
    }
}
