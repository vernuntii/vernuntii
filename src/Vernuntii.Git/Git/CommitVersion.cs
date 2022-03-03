namespace Vernuntii.Git
{
    /// <summary>
    /// Represents a semantic version with additional information regarding commit.
    /// </summary>
    public record CommitVersion : SemanticVersion
    {
        /// <summary>
        /// Represents the commit sha that is associated with this version.
        /// </summary>
        public string CommitSha { get; }

        /// <summary>
        /// Creates a new instance of <see cref="CommitVersion"/> with a commit sha.
        /// </summary>
        /// <param name="commitSha"></param>
        public CommitVersion(string commitSha) =>
            CommitSha = commitSha;

        /// <summary>
        /// Copies <paramref name="version"/> and sets the new commit sha.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="commitSha"></param>
        public CommitVersion(SemanticVersion version, string commitSha)
            : base(version) =>
            CommitSha = commitSha;

        /// <inheritdoc/>
        public override string ToString() => base.ToString();
    }
}
