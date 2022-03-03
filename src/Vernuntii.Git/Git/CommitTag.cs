namespace Vernuntii.Git
{
    /// <summary>
    /// A tag that is associated with a commit.
    /// </summary>
    public class CommitTag : ICommitTag, IEquatable<CommitTag>
    {
        /// <inheritdoc/>
        public string CommitSha { get; }
        /// <inheritdoc/>
        public string TagName { get; }

        /// <summary>
        /// Creates an instance of <see cref="CommitTag"/>.
        /// </summary>
        /// <param name="commitaSha"></param>
        /// <param name="tagName"></param>
        public CommitTag(string commitaSha, string tagName)
        {
            CommitSha = commitaSha;
            TagName = tagName;
        }

        /// <inheritdoc/>
        public bool Equals(ICommitTag? other) =>
            other is not null
            && CommitSha == other.CommitSha
            && TagName == other.TagName;

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is ICommitTag commitTag) {
                return Equals(commitTag);
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Equals(CommitTag? other) =>
            Equals(other as ICommitTag);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(CommitSha);
            hashCode.Add(TagName);
            return hashCode.ToHashCode();
        }
    }
}
