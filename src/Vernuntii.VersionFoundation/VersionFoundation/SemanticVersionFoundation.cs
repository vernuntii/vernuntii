using Vernuntii.Git;

namespace Vernuntii.VersionFoundation
{
    /// <summary>
    /// A semantic version foundation.
    /// </summary>
    public class SemanticVersionFoundation : ISemanticVersionFoundation
    {
        /// <summary>
        /// Creates a semantic version foundation.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branch"></param>
        /// <param name="creationRetentionTime">Creation retention time used to sum up with "now" (UTC) that represents the expiration time.</param>
        public static SemanticVersionFoundation Create(SemanticVersion version, IBranch branch, TimeSpan? creationRetentionTime) =>
            new SemanticVersionFoundation(
                version,
                branch.ShortBranchName,
                branch.CommitSha,
                creationRetentionTime == null ? null : DateTime.UtcNow + creationRetentionTime);

        /// <inheritdoc/>
        public SemanticVersion Version { get; }
        /// <inheritdoc/>
        public string BranchName { get; }
        /// <inheritdoc/>
        public string CommitSha { get; }
        /// <inheritdoc/>
        public DateTime? ExpirationTime { get; }
        /// <inheritdoc/>
        public DateTime? LastAccessTime { get; set; }

        /// <summary>
        /// Creates an instance of this.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branchName"></param>
        /// <param name="commitSha"></param>
        /// <param name="expirationTime"></param>
        public SemanticVersionFoundation(SemanticVersion version, string branchName, string commitSha, DateTime? expirationTime)
        {
            Version = version;
            BranchName = branchName;
            CommitSha = commitSha;
            ExpirationTime = expirationTime;
        }
    }
}
