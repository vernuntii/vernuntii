using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <summary>
    /// Provides the capability to find a version in commit history.
    /// </summary>
    public interface ICommitVersionFinder
    {
        /// <summary>
        /// Finds a version in the commit log.
        /// </summary>
        /// <param name="findingOptions"></param>
        /// <returns>The found version, otherwise null</returns>
        ICommitVersion? FindCommitVersion(CommitVersionFindingOptions findingOptions);
        /// <summary>
        /// Checks of version core (without pre-release and build) is released.
        /// </summary>
        /// <param name="version"></param>
        bool IsVersionCoreReleased(ISemanticVersion version);
    }
}
