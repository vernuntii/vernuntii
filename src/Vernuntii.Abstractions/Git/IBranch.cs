using System.Diagnostics;

namespace Vernuntii.Git
{
    /// <summary>
    /// Represents a git branch.
    /// </summary>
    public interface IBranch : IEquatable<IBranch>
    {
        /// <summary>
        /// Gets the canonical name (e.g. ref/heads/develop or HEAD)
        /// </summary>
        string LongBranchName { get; }

        /// <summary>
        /// Gets the friendly name (e.g. develop or a short sha)
        /// </summary>
        string ShortBranchName { get; }

        /// <summary>
        /// The tip commit sha of the branch.
        /// </summary>
        string CommitSha { get; }
    }
}
