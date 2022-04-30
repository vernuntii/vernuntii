using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <summary>
    /// Represents a semantic version with additional information regarding commit.
    /// </summary>
    public interface ICommitVersion : ISemanticVersion
    {
        /// <summary>
        /// Represents the commit sha that is associated with this version.
        /// </summary>
        string CommitSha { get; }
    }
}
