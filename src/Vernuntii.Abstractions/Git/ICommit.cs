namespace Vernuntii.Git
{
    /// <summary>
    /// Represents a commit.
    /// </summary>
    public interface ICommit
    {
        /// <summary>
        /// The commit sha.
        /// </summary>
        string Sha { get; }

        /// <summary>
        /// The first line of the commit message.
        /// </summary>
        string Subject { get; }
    }
}
