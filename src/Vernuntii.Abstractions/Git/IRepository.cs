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

        /// <summary>
        /// Unsets cache that will lead to reload some data on request.
        /// </summary>
        void UnsetCache();
    }
}
