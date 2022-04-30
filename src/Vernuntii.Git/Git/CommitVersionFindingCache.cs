namespace Vernuntii.Git
{
    /// <summary>
    /// Cache for <see cref="CommitVersionFindingCache"/>.
    /// </summary>
    public record CommitVersionFindingCache()
    {
        /// <summary>
        /// The found commit version.
        /// </summary>
        public ICommitVersion? CommitVersion { get; init; }

        /// <summary>
        /// True means that the version core of <see cref="CommitVersion"/>
        /// has not been already used.
        /// </summary>
        public bool CommitVersionCoreAlreadyReleased { get; init; }
    }
}
