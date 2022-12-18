namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// Mutable options class for <see cref="Repository"/>.
    /// </summary>
    public sealed class GitCommandOptions
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
        /// If <see langword="true"/> (default is <see langword="false"/>), then the above specified working tree directory gets resolved inside a post configuration with the registered service of type <see cref="IGitDirectoryResolver"/>.
        /// </summary>
        public bool ResolveGitWorkingTreeDirectory { get; set; }

        private string _gitDirectory = Directory.GetCurrentDirectory();
    }
}
