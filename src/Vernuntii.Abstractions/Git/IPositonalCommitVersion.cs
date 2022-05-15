namespace Vernuntii.Git
{
    /// <summary>
    /// A commit version relative to a start.
    /// </summary>
    public interface IPositonalCommitVersion : ICommitVersion
    {
        /// <summary>
        /// The gap at what the commit version has been found.
        /// If this version was found at the first commit then
        /// the gap would be 0.
        /// </summary>
        public int CommitGap { get; }
    }
}
