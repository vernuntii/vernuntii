using Vernuntii.IO;

namespace Vernuntii.Git
{
    /// <summary>
    /// Instance git directory with default search strategy. It searches
    /// for either .git-folder or .git-file.
    /// </summary>
    public sealed class GitDirectoryResolver : IGitDirectoryResolver
    {
        private const string GitFolderOrFileName = ".git";
        private const string DefaultVernuntiiGitFilename = "vernuntii.git";

        /// <summary>
        /// Instance instance of this type.
        /// </summary>
        public static readonly GitDirectoryResolver Default = new();

        /// <summary>
        /// 
        /// </summary>
        public string VernuntiiGitFilename { get; init; } = DefaultVernuntiiGitFilename;

        /// <summary>
        /// Searches for .git-folder or .git-file.
        /// </summary>
        /// <param name="gitPath"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public string ResolveWorkingTreeDirectory(string gitPath)
        {
            GitPathKind? pathKind = null;

            var gitDirectoryAccessor = HigherLevelDirectory.FindDirectory(directory => {
                var path = Path.Combine(directory.FullName, GitFolderOrFileName);

                // The order represents precedence
                if (File.Exists(path)) {
                    pathKind = GitPathKind.GitFile;
                } else if (Directory.Exists(path)) {
                    pathKind = GitPathKind.GitDirectory;
                } else if (File.Exists(VernuntiiGitFilename)) {
                    pathKind = GitPathKind.VernuntiiGitFile;
                }

                return pathKind.HasValue;
            }, new DirectoryInfo(gitPath), includeBeginningDirectory: true);

            var directory = gitDirectoryAccessor.GetUpwardDirectory();

            if (!pathKind.HasValue || directory == null) {
                throw new InvalidOperationException($"Could not find a parent directory containing {GitFolderOrFileName}-directory, {GitFolderOrFileName}-file or {DefaultVernuntiiGitFilename}-file");
            }

            if (pathKind == GitPathKind.VernuntiiGitFile) {
                var vernuntiiGitFile = Path.Combine(directory.FullName, DefaultVernuntiiGitFilename);
                return File.ReadLines(vernuntiiGitFile).FirstOrDefault() ?? throw new InvalidOperationException("The first line of the git pointer file was expected to be a valid git working directory");
            }

            return directory.FullName;
        }

        private enum GitPathKind
        {
            GitFile,
            GitDirectory,
            VernuntiiGitFile
        }
    }
}
