using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.Git
{
    /// <summary>
    /// A provider that returns its parameter.
    /// </summary>
    public sealed class GitDirectoryPassthrough : IGitDirectoryResolver
    {
        /// <summary>
        /// The default instance of this type.
        /// </summary>
        public readonly static GitDirectoryPassthrough Instance = new GitDirectoryPassthrough();

        private GitDirectoryPassthrough()
        {
        }

        /// <summary>
        /// Provides the top-level working tree directory.
        /// </summary>
        /// <param name="gitPath"></param>
        public string ResolveWorkingTreeDirectory(string gitPath) => gitPath;
    }
}
