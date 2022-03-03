
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
        CommitVersion? FindCommitVersion(CommitVersionFindingOptions findingOptions);
    }
}
