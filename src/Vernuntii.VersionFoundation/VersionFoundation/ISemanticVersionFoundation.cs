using Vernuntii.SemVer;

namespace Vernuntii.VersionFoundation
{
    /// <summary>
    /// The facts of produced semantic version.
    /// </summary>
    public interface ISemanticVersionFoundation
    {
        /// <summary>
        /// The version.
        /// </summary>
        ISemanticVersion Version { get; }
        /// <summary>
        /// The branch name.
        /// </summary>
        string BranchName { get; }
        /// <summary>
        /// The commit sha.
        /// </summary>
        string CommitSha { get; }
        /// <summary>
        /// Expiration time.
        /// </summary>
        public DateTime? ExpirationTime { get; }
        /// <summary>
        /// Last access time.
        /// </summary>
        public DateTime? LastAccessTime { get; set; }
    }
}
