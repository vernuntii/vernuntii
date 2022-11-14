using Vernuntii.Configuration.IO;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Lets you find a file.
    /// </summary>
    internal class FileFinder : IFileFinder
    {
        internal static readonly FileFinder Default = new();

        private static void CheckDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath)) {
                throw new ArgumentException("The directory does not exist", nameof(directoryPath));
            }
        }

        private static IFileFindingEnumerator FindUpwardDirectoryContainingFile(string directoryPath, string fileName) => new FileFindingEnumerator(
            directoryPath,
            fileName,
            UpwardDirectory.FindUpwardDirectoryContainingFile(fileName, directoryPath, includeBeginningDirectory: true).Enumerator);

        /// <inheritdoc/>
        /// <exception cref="ArgumentException"></exception>
        public IFileFindingEnumerator FindFile(string directoryPath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) {
                throw new ArgumentException("File name is null or empty", nameof(fileName));
            }

            CheckDirectoryExists(directoryPath);
            return FindUpwardDirectoryContainingFile(directoryPath, fileName);
        }

        public IFileFindingEnumerator FindFile(string directoryPath, IEnumerable<string> fileNames)
        {
            var uniqueFileNames = new HashSet<string>(fileNames);

            if (uniqueFileNames.Any(string.IsNullOrEmpty)) {
                throw new ArgumentException("One file name is null or empty", nameof(fileNames));
            }

            CheckDirectoryExists(directoryPath);
            return new AlternatingFileFindingEnumerator(uniqueFileNames.Select(fileName => FindFile(directoryPath, fileName)));
        }
    }
}
