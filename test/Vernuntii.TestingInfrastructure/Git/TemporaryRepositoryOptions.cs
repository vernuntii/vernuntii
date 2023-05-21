using Vernuntii.Git.Commands;

namespace Vernuntii.Git
{
    internal class TemporaryRepositoryOptions : GitCommandOptions
    {
        public new string WorkingTreeDirectory {
            get => base.WorkingTreeDirectory ?? throw new NotImplementedException();
            private set => base.WorkingTreeDirectory = value;
        }

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

        ///// <summary>
        ///// The working directory of a git repository.
        ///// A nested directory is tried to resolved to the root containing a .git-directory.
        ///// </summary>
        //public string WorkingTreeDirectory { get; }

        public TemporaryRepositoryOptions() =>
            WorkingTreeDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    }
}
