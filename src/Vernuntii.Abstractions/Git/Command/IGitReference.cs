namespace Vernuntii.Git.Command
{
    /// <summary>
    /// Represents a git reference.
    /// </summary>
    public interface IGitReference
    {
        /// <summary>
        /// The object name.
        /// </summary>
        string ObjectName { get; }

        /// <summary>
        /// The reference name.
        /// </summary>
        string ReferenceName { get; }
    }
}
