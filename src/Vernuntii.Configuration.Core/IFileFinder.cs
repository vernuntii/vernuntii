namespace Vernuntii.Configuration
{
    /// <summary>
    /// Provides capabilities to find a file.
    /// </summary>
    public interface IFileFinder
    {
        /// <summary>
        /// Finds a file.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName">File with extension</param>
        IFileFindingEnumerator FindFile(string directoryPath, string fileName);

        /// <summary>
        /// Finds one file.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileNames">Files with extension.</param>
        IFileFindingEnumerator FindFile(string directoryPath, IEnumerable<string> fileNames);
    }
}
