namespace Vernuntii.Git
{
    /// <summary>
    /// A tag that is associated with a commit.
    /// </summary>
    public interface ICommitTag : IEquatable<ICommitTag>
    {
        /// <summary>
        /// The commit sha the tag points to.
        /// </summary>
        string CommitSha { get; }

        /// <summary>
        /// The name of the tag.
        /// </summary>
        string TagName { get; }
    }
}
