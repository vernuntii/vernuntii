using Vernuntii.Git;
using Vernuntii.Logging;

namespace Vernuntii.MessagesProviders;

/// <summary>
/// Provides git commit messages.
/// </summary>
public class GitCommitMessagesProvider : IMessagesProvider
{
    private const int MaxDebugCommitMessageLength = 18;

    private static LoggerMessageAmendmentFactory CreateDebugMessage(ICommit commit) =>
        () => new LoggerMessageAmendment("Commit = {BranchTip}, Message = {CommitMessage}", new object[] {
            commit.Sha[..7],
            "\"" + (commit.Subject.Length >=MaxDebugCommitMessageLength
                ? commit.Subject[..MaxDebugCommitMessageLength] + "..."
                : commit.Subject) + "\""
        });

    private readonly ICommitsAccessor _commitsAccessor;
    private readonly GitCommitMessagesProviderOptions _options;

    /// <summary>
    /// Initializes an instance of <see cref="GitCommitMessagesProvider"/>.
    /// </summary>
    /// <param name="commitsAccessor"></param>
    /// <param name="options"></param>
    public GitCommitMessagesProvider(ICommitsAccessor commitsAccessor, GitCommitMessagesProviderOptions options)
    {
        _commitsAccessor = commitsAccessor;
        _options = options;
    }

    /// <summary>
    /// Initializes an instance of <see cref="GitCommitMessagesProvider"/>.
    /// </summary>
    /// <param name="commitsAccessor"></param>
    public GitCommitMessagesProvider(ICommitsAccessor commitsAccessor)
        : this(commitsAccessor, new GitCommitMessagesProviderOptions())
    {
    }

    /// <summary>
    /// Get commit messages from git.
    /// </summary>
    public IEnumerable<IMessage> GetMessages()
    {
        // Query commits from back (oldest first) to forth (newest latest)
        foreach (var commit in _commitsAccessor.GetCommits(
            branchName: _options.BranchName,
            sinceCommit: _options.SinceCommit,
            fromOldToNew: true)) {
            yield return new Message() {
                Content = commit.Subject,
                DebugMessageFactory = CreateDebugMessage(commit)
            };
        }
    }
}
