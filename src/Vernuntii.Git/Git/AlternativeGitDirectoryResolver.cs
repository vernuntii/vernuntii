using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teronis.IO;
using Vernuntii.Configuration.IO;

namespace Vernuntii.Git
{
    /// <summary>
    /// A git directory resolver that looks first for a file that may point to another git directory.
    /// </summary>
    public sealed class AlternativeGitDirectoryResolver : IGitDirectoryResolver
    {
        private readonly string _gitPointerFile;
        private readonly IGitDirectoryResolver _gitDirectoryResolver;
        private bool? _isGitPointerExisting;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="gitDirectoryResolver">
        /// The git directory resolver used for either the pointing location
        /// inside the pointer file or initial passed git path parameter.
        /// </param>
        /// <param name="gitPointerFile">
        /// The path to pointer file that has the location to the alternative git directory.
        /// </param>
        public AlternativeGitDirectoryResolver(string gitPointerFile, IGitDirectoryResolver gitDirectoryResolver)
        {
            _gitPointerFile = gitPointerFile ?? throw new ArgumentNullException(nameof(gitPointerFile));
            _gitDirectoryResolver = gitDirectoryResolver ?? throw new ArgumentNullException(nameof(gitDirectoryResolver));
        }

        private bool UseAlternativeGitPath([NotNullWhen(true)] out string? alternativeGitPath)
        {
            _isGitPointerExisting ??= File.Exists(_gitPointerFile);

            if (_isGitPointerExisting.Value) {
                alternativeGitPath = File.ReadLines(_gitPointerFile).FirstOrDefault()
                    ?? throw new InvalidOperationException("The first line of the git pointer file was expected to be a valid git working directory");

                return true;
            }

            alternativeGitPath = null;
            return false;
        }

        /// <summary>
        /// Searches for a custom git pointer file, .git-folder or .git-file.
        /// </summary>
        /// <param name="gitPath"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public string ResolveWorkingTreeDirectory(string gitPath)
        {
            if (UseAlternativeGitPath(out var alternativeGitPath)) {
                return _gitDirectoryResolver.ResolveWorkingTreeDirectory(alternativeGitPath);
            } else {
                return _gitDirectoryResolver.ResolveWorkingTreeDirectory(gitPath);
            }
        }
    }
}
