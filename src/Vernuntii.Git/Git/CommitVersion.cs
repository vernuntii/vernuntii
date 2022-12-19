using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <summary>
    /// Represents a semantic version with additional information regarding commit.
    /// </summary>
    public record CommitVersion : SemanticVersion, ICommitVersion
    {
        /// <inheritdoc/>
        public string CommitSha { get; }

        /// <summary>
        /// Creates a new instance of <see cref="CommitVersion"/> with a commit sha.
        /// </summary>
        /// <param name="commitSha"></param>
        public CommitVersion(string commitSha) =>
            CommitSha = commitSha ?? throw new ArgumentNullException(nameof(commitSha));

        /// <summary>
        /// Copies <paramref name="version"/> and sets the new commit sha.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="commitSha"></param>
        public CommitVersion(string commitSha, ISemanticVersion version)
            : base(version) =>
            CommitSha = commitSha ?? throw new ArgumentNullException(nameof(commitSha));

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] ICommitVersion? other) =>
            base.Equals(other)
            && StringComparer.InvariantCultureIgnoreCase.Equals(CommitSha, other.CommitSha);

        /// <inheritdoc/>
        public sealed override bool Equals([NotNullWhen(true)] ISemanticVersion? other) =>
            other is ICommitVersion otherCommitVersion
            ? Equals(otherCommitVersion)
            : base.Equals(other);

        /// <inheritdoc/>
        public virtual bool Equals([NotNullWhen(true)] CommitVersion? other) =>
            Equals((ICommitVersion?)other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(
            base.GetHashCode(),
            CommitSha.GetHashCode(StringComparison.InvariantCultureIgnoreCase));

        /// <inheritdoc/>
        public override string ToString() =>
            this.Format(SemanticVersionFormat.SemanticVersion);
    }
}
