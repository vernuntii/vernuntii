namespace Vernuntii.MessagesProviders
{
    /// <summary>
    /// The options for <see cref="GitCommitMessagesProvider"/>.
    /// </summary>
    public class GitCommitMessagesProviderOptions
    {
        /// <summary>
        /// The since-commit where to start reading from.
        /// </summary>
        public string? SinceCommit { get; set; }
        /// <summary>
        /// The branch reading the commits from.
        /// </summary>
        public string? BranchName { get; set; }
    }
}
