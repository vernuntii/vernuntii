namespace Vernuntii.Git
{
    /// <summary>
    /// A minimal functional repository.
    /// </summary>
    public interface IRepository : ICommitsAccessor, ICommitTagsAccessor, ICommitVersionsAccessor, IBranchesAccessor
    {
        /// <summary>
        /// Gets the .git directory.
        /// </summary>
        string GetGitDirectory();
    }
}
