using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// The default git command factory.
    /// </summary>
    public class DefaultGitCommandFactory : IGitCommandFactory
    {
        /// <summary>
        /// The default instance of this type.
        /// </summary>
        public readonly static DefaultGitCommandFactory Default = new DefaultGitCommandFactory();

        /// <inheritdoc/>
        public IGitCommand CreateCommand(string gitDirectory) =>
            new GitCommand(gitDirectory);
    }
}
