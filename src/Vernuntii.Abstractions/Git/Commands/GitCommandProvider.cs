using System;
using System.Collections.Generic;
using System.Text;

namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// Instead of creating it provides the instance passed to constructor.
    /// </summary>
    public sealed class GitCommandProvider : IGitCommandFactory
    {
        /// <summary>
        /// The git command passed by constructor.
        /// </summary>
        public IGitCommand Command { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="command"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public GitCommandProvider(IGitCommand command) =>
            Command = command ?? throw new ArgumentNullException(nameof(command));

        /// <summary>
        /// Does not create a command but returns <see cref="Command"/>.
        /// </summary>
        /// <param name="gitDirectory"></param>
        public IGitCommand CreateCommand(string gitDirectory) => Command;
    }
}
