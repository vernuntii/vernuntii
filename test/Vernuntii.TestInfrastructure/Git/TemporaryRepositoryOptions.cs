using Vernuntii.Git.Commands;

namespace Vernuntii.Git
{
    internal class TemporaryRepositoryOptions
    {
        public GitCommandOptions CommandOptions { get; }

        /// <summary>
        /// If true the directory gets deleted on dispose.
        /// Instance is <see langword="true"/>.
        /// </summary>
        public bool DeleteOnDispose { get; set; } = true;

        /// <summary>
        /// If true the repository directory gets only deleted when 
        /// its path begins with <see cref="Path.GetTempPath"/>.
        /// No Effect if <see cref="DeleteOnDispose"/> is <see langword="false"/>.
        /// Instance is <see langword="true"/>.
        /// </summary>
        public bool DeleteOnlyTempDirectory { get; set; } = true;

        public CloneOptions? CloneOptions { get; set; }

        public TemporaryRepositoryOptions()
        {
            CommandOptions = new GitCommandOptions() {
                GitWorkingTreeDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
            };
        }

        public static implicit operator GitCommandOptions(TemporaryRepositoryOptions repositoryOptions) =>
            repositoryOptions.CommandOptions;
    }
}
