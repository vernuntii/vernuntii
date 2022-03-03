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
        public string GitDirectory {
            get => _gitDirectory;
            set => _gitDirectory = value ?? throw new ArgumentNullException(nameof(value), "Git directory cannot be null");
        }

        private string _gitDirectory = Directory.GetCurrentDirectory();
    }
}
