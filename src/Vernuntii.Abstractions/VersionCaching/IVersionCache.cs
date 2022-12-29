using Vernuntii.SemVer;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The attributes regarding the version foundation.
    /// </summary>
    public interface IVersionCache
    {
        /// <summary>
        /// <summary>
        /// The version.
        /// </summary>
        ISemanticVersion Version { get; }

        /// <summary>
        /// The branch name.
        /// </summary>
        string BranchName { get; }

        /// <summary>
        /// The last commit of branch.
        /// </summary>
        string BranchTip { get; }

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

