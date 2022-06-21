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
    public sealed class InOutGitDirectoryProvider : IGitDirectoryResolver
    {
        /// <summary>
        /// The default instance of this type.
        /// </summary>
        public readonly static InOutGitDirectoryProvider Instance = new InOutGitDirectoryProvider();

        private InOutGitDirectoryProvider()
        {
        }

        /// <summary>
        /// Provides the git directory that has been passed.
        /// </summary>
        /// <param name="gitPath"></param>
        public string ResolveGitDirectory(string gitPath) => gitPath;
    }
}
