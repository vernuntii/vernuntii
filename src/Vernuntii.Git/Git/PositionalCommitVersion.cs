using System.Diagnostics.CodeAnalysis;
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
        public PositionalCommitVersion(string commitSha, int commitGap, ISemanticVersion version)
            : base(commitSha, version) =>
            CommitGap = commitGap;

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] IPositonalCommitVersion? other) =>
            base.Equals(other)
            && CommitGap == other.CommitGap;

        /// <inheritdoc/>
        public sealed override bool Equals([NotNullWhen(true)] ICommitVersion? other) =>
            other is ICommitVersion otherCommitVersion
            ? Equals(otherCommitVersion)
            : base.Equals(other);

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] PositionalCommitVersion? other) =>
            Equals((IPositonalCommitVersion?)other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(
            base.GetHashCode(),
            CommitGap.GetHashCode());

        /// <inheritdoc/>
        public override string ToString() =>
            this.Format(SemanticVersionFormat.SemanticVersion);
    }
}
