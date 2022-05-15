using System.Text.Json.Serialization;
using Vernuntii.Git;
using Vernuntii.SemVer;

namespace Vernuntii.VersionFoundation
{
    /// <summary>
    /// A semantic version foundation.
    /// </summary>
    public class DefaultVersionFoundation : IVersionFoundation<SemanticVersion>, IVersionFoundation
    {
        /// <summary>
        /// Creates a semantic version foundation.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branch"></param>
        /// <param name="creationRetentionTime">Creation retention time used to sum up with "now" (UTC) that represents the expiration time.</param>
        public static DefaultVersionFoundation Create(ISemanticVersion version, IBranch branch, TimeSpan? creationRetentionTime) =>
            new DefaultVersionFoundation(
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

        ISemanticVersion IVersionFoundation<ISemanticVersion>.Version => Version;

        /// <summary>
        /// Creates an instance of this.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branchName"></param>
        /// <param name="commitSha"></param>
        /// <param name="expirationTime"></param>
        [JsonConstructor]
        public DefaultVersionFoundation(SemanticVersion version, string branchName, string commitSha, DateTime? expirationTime)
        {
            Version = version;
            BranchName = branchName;
            CommitSha = commitSha;
            ExpirationTime = expirationTime;
        }

        /// <summary>
        /// Creates an instance of this.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branchName"></param>
        /// <param name="commitSha"></param>
        /// <param name="expirationTime"></param>
        public DefaultVersionFoundation(ISemanticVersion version, string branchName, string commitSha, DateTime? expirationTime)
            : this(new SemanticVersion(version), branchName, commitSha, expirationTime)
        {
        }
    }
}
