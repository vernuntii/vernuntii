namespace Vernuntii.Git
{
    internal class TemporaryRepositoryOptions
    {
        public RepositoryOptions RepositoryOptions { get; }

        /// <summary>
        /// If true the directory gets deleted on dispose.
        /// Default is <see langword="true"/>.
        /// </summary>
        public bool DeleteOnDispose { get; set; } = true;

        /// <summary>
        /// If true the repository directory gets only deleted when 
        /// its path begins with <see cref="Path.GetTempPath"/>.
        /// No Effect if <see cref="DeleteOnDispose"/> is <see langword="false"/>.
        /// Default is <see langword="true"/>.
        /// </summary>
        public bool DeleteOnlyTempDirectory { get; set; } = true;

        public TemporaryRepositoryOptions()
        {
            RepositoryOptions = new RepositoryOptions() {
                GitDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
            };
        }
    }
}
