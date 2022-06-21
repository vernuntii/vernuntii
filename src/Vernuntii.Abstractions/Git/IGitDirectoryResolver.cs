namespace Vernuntii.Git
{
    /// <summary>
    /// Has the ability to resolve to the git working directory.
    /// </summary>
    public interface IGitDirectoryResolver
    {
        /// <summary>
        /// Gets the git working directory.
        /// </summary>
        /// <param name="gitPath"></param>
        public string ResolveGitDirectory(string gitPath);
    }
}
