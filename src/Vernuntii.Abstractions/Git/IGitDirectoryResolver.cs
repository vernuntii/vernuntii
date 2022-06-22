namespace Vernuntii.Git
{
    /// <summary>
    /// Has the ability to resolve git-specific directories.
    /// </summary>
    public interface IGitDirectoryResolver
    {
        /// <summary>
        /// Gets the top-level of the working tree.
        /// </summary>
        /// <param name="gitPath"></param>
        public string ResolveWorkingTreeDirectory(string gitPath);
    }
}
