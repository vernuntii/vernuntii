using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <summary>
    /// A commit version relative to a start.
    /// </summary>
    public record class PositionalCommitVersion : CommitVersion, IPositonalCommitVersion
    {
        /// <inheritdoc/>
        public int CommitGap { get; }

        /// <inheritdoc/>
        protected PositionalCommitVersion(PositionalCommitVersion original)
            : base(original) =>
            CommitGap = original.CommitGap;

        /// <inheritdoc/>
        public PositionalCommitVersion(string commitSha, int commitGap)
            : base(commitSha) =>
            CommitGap = commitGap;

        /// <inheritdoc/>
        public PositionalCommitVersion(ISemanticVersion version, string commitSha, int commitGap)
            : base(version, commitSha) =>
            CommitGap = commitGap;

        /// <inheritdoc/>
        public override string ToString() =>
            this.Format(SemanticVersionFormat.SemanticVersion);
    }
}
