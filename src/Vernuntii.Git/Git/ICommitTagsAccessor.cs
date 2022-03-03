namespace Vernuntii.Git
{
    /// <summary>
    /// Owns the capabilities to access the tags.
    /// </summary>
    public interface ICommitTagsAccessor
    {
        /// <summary>
        /// Gets the tags from git repository.
        /// </summary>
        IEnumerable<ICommitTag> GetCommitTags();
    }
}
