using Vernuntii.Git.Commands;

namespace Vernuntii.Git
{
    /// <summary>
    /// Mutable options class for <see cref="Repository"/>.
    /// </summary>
    public sealed class RepositoryOptions
    {
        /// <summary>
        /// The working directory (default is current directory) of a git repository.
        /// A nested directory is tried to resolved to the root containing a .git-directory.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public string GitWorkingTreeDirectory {
            get => _gitDirectory;
            set => _gitDirectory = value ?? throw new ArgumentNullException(nameof(value), "Git directory cannot be null");
        }

        /// <summary>
        /// The git directory resolver. It will be used to resolve the actual
        /// git directory. <see cref="GitWorkingTreeDirectory"/> will be the input.
        /// </summary>
        public IGitDirectoryResolver GitDirectoryResolver {
            get => _gitDirectoryResolver;
            set => _gitDirectoryResolver = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Pre-initialized git command.
        /// </summary>
        public IGitCommandFactory GitCommandFactory {
            get => _gitCommandFactory;
            set => _gitCommandFactory = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string _gitDirectory = Directory.GetCurrentDirectory();
        private IGitDirectoryResolver _gitDirectoryResolver = DefaultGitDirectoryResolver.Default;
        private IGitCommandFactory _gitCommandFactory = DefaultGitCommandFactory.Default;
    }
}
