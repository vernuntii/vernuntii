namespace Vernuntii.Git
{
    /// <summary>
    /// Provides the capability to find a version in the commit history.
    /// </summary>
    public interface ICommitVersionFinder
    {
        /// <summary>
        /// Tries to find a version in the commit log.
        /// </summary>
        /// <param name="options"></param>
        /// <returns>The found version, otherwise null</returns>
        IPositonalCommitVersion? FindCommitVersion(ICommitVersionFindOptions options);
    }
}
