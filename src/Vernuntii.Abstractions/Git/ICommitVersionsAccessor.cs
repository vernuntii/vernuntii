namespace Vernuntii.Git
{
    /// <summary>
    /// Owns the capabilities to access commits that are tagged.
    /// </summary>
    public interface ICommitVersionsAccessor
    {
        /// <summary>
        /// Gets versions which are each associated to a commit.
        /// </summary>
        IReadOnlyList<ICommitVersion> GetCommitVersions();
    }
}
