namespace Vernuntii.IO
{
    /// <summary>
    /// Utility methods for finding a higher-level directory.
    /// </summary>
    public static class HigherLevelDirectory
    {
        private static IEnumerator<DirectoryInfo?> EnumerateHigherLevelDirectories(HigherLevelDirectoryPredicate directoryPredicate, DirectoryInfo beginningDirectory, bool includeBeginningDirectory)
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
        public static HigherLevelDirectoryAccessor FindDirectory(HigherLevelDirectoryPredicate directoryPredicate, DirectoryInfo beginningDirectory, bool includeBeginningDirectory = false) =>
            new(EnumerateHigherLevelDirectories(directoryPredicate, beginningDirectory, includeBeginningDirectory));

        private static bool ContainsFile(DirectoryInfo directoryInfo, string fileNameWithExtension)
        {
            var directoryWithFileName = Path.Combine(directoryInfo.FullName, fileNameWithExtension);
            return File.Exists(directoryWithFileName);
        }

        /// <summary>
        /// Gets the absolute directory path of file above beginning from the parent directory of <paramref name="beginningDirectory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static HigherLevelDirectoryAccessor FindDirectoryWithFile(string fileNameWithExtension, DirectoryInfo beginningDirectory, bool includeBeginningDirectory = false) =>
            FindDirectory((directoryInfo) => ContainsFile(directoryInfo, fileNameWithExtension), beginningDirectory, includeBeginningDirectory);

        /// <summary>
        /// Gets the absolute directory path of file above beginning from the parent directory of <paramref name="directory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static HigherLevelDirectoryAccessor FindDirectoryWithFile(string fileNameWithExtension, string directory, bool includeBeginningDirectory = false)
        {
            directory = directory ?? throw new ArgumentNullException(directory);
            return FindDirectoryWithFile(fileNameWithExtension, new DirectoryInfo(directory), includeBeginningDirectory: includeBeginningDirectory);
        }

        /// <summary>
        /// Gets the absolute directory path of file above beginning from the parent directory of the entry point, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static HigherLevelDirectoryAccessor FindDirectoryWithFile(string fileNameWithExtension, bool includeBeginningDirectory = false) =>
            FindDirectoryWithFile(fileNameWithExtension, AppDomain.CurrentDomain.BaseDirectory!, includeBeginningDirectory);

        private static bool isDirectoryOfDirectoryAbove(DirectoryInfo directoryInfo, string directoryName)
        {
            var directoryWithFileName = Path.Combine(directoryInfo.FullName, directoryName);
            return Directory.Exists(directoryWithFileName);
        }

        /// <summary>
        /// Gets the absolute directory path of directory above beginning from the parent directory of <paramref name="beginningDirectory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static HigherLevelDirectoryAccessor FindDirectoryWithDirectory(string directoryName, DirectoryInfo beginningDirectory, bool includeBeginningDirectory = false) =>
            FindDirectory((directoryInfo) => isDirectoryOfDirectoryAbove(directoryInfo, directoryName), beginningDirectory, includeBeginningDirectory);

        /// <summary>
        /// Gets the absolute directory path of directory above beginning from the parent directory of <paramref name="directory"/>, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static HigherLevelDirectoryAccessor FindDirectoryWithDirectory(string directoryName, string directory, bool includeBeginningDirectory = false)
        {
            directory = directory ?? throw new ArgumentNullException(directory);
            return FindDirectoryWithDirectory(directoryName, new DirectoryInfo(directory), includeBeginningDirectory: includeBeginningDirectory);
        }

        /// <summary>
        /// Gets the absolute directory path of directory above beginning from the parent directory of the entry point, unless <paramref name="includeBeginningDirectory"/> is true.
        /// </summary>
        public static HigherLevelDirectoryAccessor FindDirectoryWithDirectory(string directoryName, bool includeBeginningDirectory = false) =>
            FindDirectoryWithDirectory(directoryName, AppDomain.CurrentDomain.BaseDirectory!, includeBeginningDirectory);
    }
}
