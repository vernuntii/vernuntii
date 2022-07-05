using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// A low-level access to git.
    /// </summary>
    public interface IGitCommand : IDisposable
    {
        /// <summary>
        /// The working directory at time of the creation of this instance.
        /// </summary>
        string WorkingTreeDirectory { get; }

        /// <summary>
        /// Gets the active branch name.
        /// </summary>
        string GetActiveBranchName();

        /// <summary>
        /// Gets branches.
        /// </summary>
        IEnumerable<IBranch> GetBranches();

        /// <summary>
        /// Gets commits.
        /// </summary>
        /// <param name="branchName"></param>
        /// <param name="sinceCommit"></param>
        /// <param name="reverse"></param>
        IEnumerable<ICommit> GetCommits(string? branchName, string? sinceCommit, bool reverse);

        /// <summary>
        /// Gets commit tags.
        /// </summary>
        IEnumerable<ICommitTag> GetCommitTags();

        /// <summary>
        /// Gets the .git-directory.
        /// </summary>
        string GetGitDirectory();

        /// <summary>
        /// Checks if head is detached.
        /// </summary>
        bool IsHeadDetached();

        /// <summary>
        /// Checks whether the repository is shallow.
        /// </summary>
        bool IsShallow();

        /// <summary>
        /// Tries to resolve a name to a reference.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="showRefLimit"></param>
        /// <param name="reference"></param>
        /// <returns><see langword="true"/> when the name could be resolved.</returns>
        bool TryResolveReference(string? name, ShowRefLimit showRefLimit, [NotNullWhen(true)] out IGitReference? reference);
    }
}
