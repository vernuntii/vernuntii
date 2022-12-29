using System.Text.Json.Serialization;
using Vernuntii.Git;
using Vernuntii.SemVer;
using Vernuntii.SemVer.Json.System;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// A semantic version foundation.
    /// </summary>
    public class DefaultVersionCache : IVersionCache
    {
        /// <summary>
        /// Creates a semantic version foundation.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branch"></param>
        /// <param name="creationRetentionTime">Creation retention time used to sum up with "now" (UTC) that represents the expiration time.</param>
        public static DefaultVersionCache Create(ISemanticVersion version, IBranch branch, TimeSpan? creationRetentionTime) => new(
            version,
            branch.ShortBranchName,
            branch.CommitSha,
            creationRetentionTime == null ? null : DateTime.UtcNow + creationRetentionTime);

        /// <inheritdoc/>
        public ISemanticVersion Version { get; }
        /// <inheritdoc/>
        public string BranchName { get; }
        /// <inheritdoc/>
        public string BranchTip { get; }
        /// <inheritdoc/>
        public DateTime? ExpirationTime { get; }
        /// <inheritdoc/>
        public DateTime? LastAccessTime { get; set; }

        /// <summary>
        /// Creates an instance of this.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="branchName"></param>
        /// <param name="branchTip"></param>
        /// <param name="expirationTime"></param>
        [JsonConstructor]
        public DefaultVersionCache(ISemanticVersion version, string branchName, string branchTip, DateTime? expirationTime)
        {
            Version = version;
            BranchName = branchName;
            BranchTip = branchTip;
            ExpirationTime = expirationTime;
        }

        ///// <summary>
        ///// Creates an instance of this.
        ///// </summary>
        ///// <param name="version"></param>
        ///// <param name="branchName"></param>
        ///// <param name="branchTip"></param>
        ///// <param name="expirationTime"></param>
        //public DefaultVersionCache(ISemanticVersion version, string branchName, string branchTip, DateTime? expirationTime)
        //    : this(version, branchName, branchTip, expirationTime)
        //{
        //}
    }
}
