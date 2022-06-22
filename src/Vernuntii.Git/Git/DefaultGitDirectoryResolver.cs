using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teronis.IO;
using Vernuntii.Configuration.IO;

namespace Vernuntii.Git
{
    /// <summary>
    /// Instance git directory with default search strategy. It searches
    /// for either .git-folder or .git-file.
    /// </summary>
    public sealed class DefaultGitDirectoryResolver : IGitDirectoryResolver
    {
        /// <summary>
        /// Instance instance of this type.
        /// </summary>
        public readonly static DefaultGitDirectoryResolver Default = new DefaultGitDirectoryResolver();

        private const string GitFolderOrFileName = ".git";

        /// <summary>
        /// Searches for .git-folder or .git-file.
        /// </summary>
        /// <param name="gitPath"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public string ResolveWorkingTreeDirectory(string gitPath)
        {
            var gitDirectoryAccessor = UpwardDirectory.FindUpwardDirectory(directory => {
                var path = Path.Combine(directory.FullName, GitFolderOrFileName);
                return File.Exists(path) || Directory.Exists(path);
            }, new DirectoryInfo(gitPath), includeBeginningDirectory: true);

            return gitDirectoryAccessor.GetUpwardDirectory()?.FullName
                ?? throw new InvalidOperationException("Could not find a parent directory containing .git-directory or .git-file");
        }
    }
}
