namespace Vernuntii.Configuration
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
        string GetCurrentFilePath();

        /// <summary>
        /// Gets file path of upward directory info.
        /// </summary>
        /// <returns>file path</returns>
        string GetUpwardFilePath();
    }
}
