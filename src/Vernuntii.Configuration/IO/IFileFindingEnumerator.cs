namespace Vernuntii.IO
{
    /// <summary>
    /// Enumerates directories and can get file path.
    /// </summary>
    public interface IFileFindingEnumerator : IEnumerator<DirectoryInfo?>
    {
        /// <summary>
        /// Gets file path of current directory info.
        /// </summary>
        /// <returns>file path</returns>
        string GetCurrentLevelFilePath();

        /// <summary>
        /// Gets file path of upward directory info.
        /// </summary>
        /// <returns>file path</returns>
        string GetHigherLevelFilePath();
    }
}
