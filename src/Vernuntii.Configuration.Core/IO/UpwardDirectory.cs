namespace Vernuntii.Configuration.IO
{
    /// <summary>
    /// Utility methods for finding a directory upwards.
    /// </summary>
    public static class UpwardDirectory
    {
        private static IEnumerator<DirectoryInfo?> CreateUpwardDirectoryEnumerator(UpwardDirectoryPredicate directoryPredicate, DirectoryInfo beginningDirectory, bool includeBeginningDirectory)
        {
            var currentDirectory = beginningDirectory ?? throw new ArgumentNullException(nameof(beginningDirectory));

            if (!includeBeginningDirectory) {
                currentDirectory = currentDirectory.Parent;
            }

            while (currentDirectory != null) {
                var success = directoryPredicate(currentDirectory);

                if (success) {
                    yield return currentDirectory;
                    yield break;
                } else {
                    yield return null;
                }

                currentDirectory = currentDirectory.Parent;
            }
        }

        /// <summary>
        /// Gets the absolute directory path of file or directory above beginning from the parent directory of <paramref name="beginningDirectory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectory(UpwardDirectoryPredicate directoryPredicate, DirectoryInfo beginningDirectory, bool includeBeginningDirectory = false) =>
            new UpwardDirectoryAccessor(CreateUpwardDirectoryEnumerator(directoryPredicate, beginningDirectory, includeBeginningDirectory));

        private static bool ContainsFile(DirectoryInfo directoryInfo, string fileNameWithExtension)
        {
            var directoryWithFileName = Path.Combine(directoryInfo.FullName, fileNameWithExtension);
            return File.Exists(directoryWithFileName);
        }

        /// <summary>
        /// Gets the absolute directory path of file above beginning from the parent directory of <paramref name="beginningDirectory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectoryContainingFile(string fileNameWithExtension, DirectoryInfo beginningDirectory, bool includeBeginningDirectory = false) =>
            FindUpwardDirectory((directoryInfo) => ContainsFile(directoryInfo, fileNameWithExtension), beginningDirectory, includeBeginningDirectory);

        /// <summary>
        /// Gets the absolute directory path of file above beginning from the parent directory of <paramref name="directory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectoryContainingFile(string fileNameWithExtension, string directory, bool includeBeginningDirectory = false)
        {
            directory = directory ?? throw new ArgumentNullException(directory);
            return FindUpwardDirectoryContainingFile(fileNameWithExtension, new DirectoryInfo(directory), includeBeginningDirectory: includeBeginningDirectory);
        }

        /// <summary>
        /// Gets the absolute directory path of file above beginning from the parent directory of the entry point, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectoryContainingFile(string fileNameWithExtension, bool includeBeginningDirectory = false) =>
            FindUpwardDirectoryContainingFile(fileNameWithExtension, AppDomain.CurrentDomain.BaseDirectory!, includeBeginningDirectory);

        private static bool isDirectoryOfDirectoryAbove(DirectoryInfo directoryInfo, string directoryName)
        {
            var directoryWithFileName = Path.Combine(directoryInfo.FullName, directoryName);
            return Directory.Exists(directoryWithFileName);
        }

        /// <summary>
        /// Gets the absolute directory path of directory above beginning from the parent directory of <paramref name="beginningDirectory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectoryContainingDirectory(string directoryName, DirectoryInfo beginningDirectory, bool includeBeginningDirectory = false) =>
            FindUpwardDirectory((directoryInfo) => isDirectoryOfDirectoryAbove(directoryInfo, directoryName), beginningDirectory, includeBeginningDirectory);

        /// <summary>
        /// Gets the absolute directory path of directory above beginning from the parent directory of <paramref name="directory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectoryContainingDirectory(string directoryName, string directory, bool includeBeginningDirectory = false)
        {
            directory = directory ?? throw new ArgumentNullException(directory);
            return FindUpwardDirectoryContainingDirectory(directoryName, new DirectoryInfo(directory), includeBeginningDirectory: includeBeginningDirectory);
        }

        /// <summary>
        /// Gets the absolute directory path of directory above beginning from the parent directory of the entry point, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static UpwardDirectoryAccessor FindUpwardDirectoryContainingDirectory(string directoryName, bool includeBeginningDirectory = false) =>
            FindUpwardDirectoryContainingDirectory(directoryName, AppDomain.CurrentDomain.BaseDirectory!, includeBeginningDirectory);
    }
}
