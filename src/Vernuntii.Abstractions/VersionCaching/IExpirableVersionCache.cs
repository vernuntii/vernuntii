using System;
using System.Collections.Generic;
using System.Text;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The attributes regarding the version foundation.
    /// </summary>
    public interface IExpirableVersionCache
    {
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
