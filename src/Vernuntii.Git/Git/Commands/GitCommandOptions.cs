namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// Mutable options class for <see cref="Repository"/>.
    /// </summary>
    public class GitCommandOptions
    {
        /// <summary>
        /// The working directory of a git repository.
        /// A nested directory is tried to resolved to the root containing a .git-directory.
        /// </summary>
        public string? WorkingTreeDirectory { get; set; }
    }
}
