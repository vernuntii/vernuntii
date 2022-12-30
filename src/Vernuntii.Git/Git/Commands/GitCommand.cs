using System.Diagnostics.CodeAnalysis;
using System.Text;
using Kenet.SimpleProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vernuntii.Text;

namespace Vernuntii.Git.Commands;

/// <summary>
/// The git command with limited capabilities.
/// </summary>
public class GitCommand : IGitCommand
{
    private bool _isDisposed;
    private Lazy<LibGit2Command> _libGit2;

    /// <inheritdoc/>
    public string WorkingTreeDirectory =>
        _workingTreeDirectory
        ??= _workingTreeDirectoryFunc?.Invoke()
        ?? throw new NotImplementedException();

    private string? _workingTreeDirectory;
    private Func<string>? _workingTreeDirectoryFunc;

    private GitCommand()
    {
        _libGit2 = new Lazy<LibGit2Command>(() => new LibGit2Command(WorkingTreeDirectory));
    }

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="workingTreeDirectory"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal GitCommand(string workingTreeDirectory) : this() =>
        _workingTreeDirectory = workingTreeDirectory ?? throw new ArgumentNullException(nameof(workingTreeDirectory));

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="gitDirectoryResolver"></param>
    [ActivatorUtilitiesConstructor]
    public GitCommand(IOptionsSnapshot<GitCommandOptions> options, IGitDirectoryResolver gitDirectoryResolver) : this() =>
        _workingTreeDirectoryFunc = () => gitDirectoryResolver.ResolveWorkingTreeDirectory(options.Value.WorkingTreeDirectory ?? Directory.GetCurrentDirectory());

    private GitProcessStartInfo CreateStartInfo(string? args) => new(args, WorkingTreeDirectory);

    /// <summary>
    /// Executes the git command.
    /// </summary>
    /// <param name="args">The arguments next to the git-command.</param>
    /// <param name="expectedExitCode">The expected exit code.</param>
    /// <returns>The exit code.</returns>
    /// <exception cref="BadExitCodeException"></exception>
    protected int ExecuteCommand(string args, int expectedExitCode = 0) => ProcessExecutorBuilder.CreateDefault(CreateStartInfo(args))
        .WithExitCode(expectedExitCode)
        .RunToCompletion();

    /// <summary>
    /// Executes the git command and then reads the output.
    /// </summary>
    /// <param name="args">The arguments next to the git-command.</param>
    /// <returns>The read output.</returns>
    protected string ExecuteCommandThenReadOutput(string? args)
    {
        using var boundary = new ProcessBoundary();

        ProcessExecutorBuilder.CreateDefault(CreateStartInfo(args))
            .WriteToBufferReader(builder => builder.AddOutputWriter, out var bufferOwner, boundary)
            .RunToCompletion();

        return Encoding.UTF8.GetString(bufferOwner.WrittenMemory.Span).TrimEnd();
    }

    /// <summary>
    /// Executes the command and then reads the outputted lines.
    /// </summary>
    /// <param name="args">The arguments next to the git-command.</param>
    /// <returns>The outputted lines.</returns>
    protected List<string> ExecuteCommandThenReadLines(string? args)
    {
        using var boundary = new ProcessBoundary();

        ProcessExecutorBuilder.CreateDefault(CreateStartInfo(args))
            .WriteToBufferReader(builder => builder.AddOutputWriter, out var bufferOwner, boundary)
            .RunToCompletion();

        return Encoding.UTF8.GetString(bufferOwner.WrittenMemory.Span).Split("\n").ToList();
    }

    /// <inheritdoc/>
    public bool IsHeadDetached() => _libGit2.Value.IsHeadDetached();

    /// <inheritdoc/>
    public string GetGitDirectory()
    {
        var possibleGitDirectory = Path.Combine(WorkingTreeDirectory, ".git");

        // To minimize version cache check time, we do fast-check
        if (Directory.Exists(possibleGitDirectory)) {
            return possibleGitDirectory;
        }

        return ExecuteCommandThenReadOutput("rev-parse --absolute-git-dir");
    }

    /// <inheritdoc/>
    public bool TryResolveReference(
        string? name,
        ShowRefLimit showRefLimit,
        [NotNullWhen(true)] out IGitReference? reference)
    {
        var lines = ExecuteCommandThenReadLines(BuildArguments());
        lines.RemoveAll(string.IsNullOrWhiteSpace);

        if (lines.Count > 1) {
            throw new ArgumentException("Reference name \"" + name + "\" could not be resolved to single reference");
        }

        if (lines.Count == 1) {
            var result = GitReference.TryParse(lines[0], out var gitReference);
            reference = gitReference;
            return result;
        }

        reference = null;
        return false;

        string BuildArguments()
        {
            var stringBuilder = CultureStringBuilder.Invariant();
            stringBuilder.Append($"show-ref {name}");

            if (showRefLimit.HasFlag(ShowRefLimit.Tags)) {
                stringBuilder.Append(" --tags");
            }

            if (showRefLimit.HasFlag(ShowRefLimit.Heads)) {
                stringBuilder.Append(" --heads");
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>Gets name of active branch.</summary>
    /// <returns>The full reference name.</returns>
    public virtual string GetActiveBranchName() => ExecuteCommandThenReadOutput("rev-parse --symbolic-full-name HEAD");

    /// <inheritdoc/>
    public IEnumerable<IBranch> GetBranches()
    {
        foreach (var line in ExecuteCommandThenReadLines("branch --all --format=\"%(objectname) %(refname) %(refname:short)\"")) {
            if (!string.IsNullOrWhiteSpace(line)) {
                if (line.Contains("HEAD detached at", StringComparison.Ordinal)) {
                    yield return new Branch(line.Substring(0, line.IndexOf(' ', StringComparison.Ordinal)), "HEAD", "HEAD", "HEAD");
                    continue;
                }

                var strArray = line.Split(' ');
                var referenceName = strArray[1];

                yield return new Branch(
                    commitSha: strArray[0],
                    longBranchName: referenceName,
                    shortBranchName: strArray[2],
                    identifier: referenceName);
            }
        }
    }

    /// <inheritdoc/>
    public virtual IEnumerable<ICommitTag> GetCommitTags()
    {
        foreach (var line in ExecuteCommandThenReadLines("tag --format=\"%(if)%(*objectname)%(then)%(*objectname)%(else)%(objectname)%(end) %(refname:short)\"")) {
            if (string.IsNullOrWhiteSpace(line)) {
                continue;
            }

            var strArray = line.Split(' ');
            yield return new CommitTag(strArray[0], strArray[1]);
        }
    }

    /// <inheritdoc/>
    public virtual IEnumerable<ICommit> GetCommits(
        string? branchName,
        string? sinceCommit,
        bool reverse)
    {
        foreach (var line in ExecuteCommandThenReadLines(BuildArguments())) {
            if (line == null) {
                continue;
            }

            var firstSpace = line.IndexOf(' ', StringComparison.Ordinal);

            if (firstSpace == -1) {
                break;
            }

            var sha = line[..firstSpace];
            var subject = line[(firstSpace + 1)..];
            yield return new Commit(sha, subject);
        }

        string BuildArguments()
        {
            var stringBuilder = CultureStringBuilder.Invariant();
            stringBuilder.Append($"log {branchName ?? "HEAD"}");

            if (!string.IsNullOrEmpty(sinceCommit)) {
                stringBuilder.Append($" --not {sinceCommit}");
            }

            stringBuilder.Append(" --format=\"%H %s\"");

            if (reverse) {
                stringBuilder.Append(" --reverse");
            }

            return stringBuilder.ToString();
        }
    }

    /// <inheritdoc/>
    public bool IsShallow() =>
        _libGit2.Value.IsShallow();

    private record GitProcessStartInfo : SimpleProcessStartInfo
    {
        private const string GitCommandName = "git";

        public GitProcessStartInfo(string? args = null, string? workingDirectory = null)
            : base(GitCommandName)
        {
            Arguments = args;
            WorkingDirectory = workingDirectory;
        }
    }

    internal readonly struct NullableQuote
    {
        public static NullableQuote DoubleQuoted(string content) =>
            new($"\"{content}\"");

        public static NullableQuote SingleQuoted(string content) =>
            new($"'{content}'");

        public string? Content { get; }

        public NullableQuote(string content) =>
            Content = content;

        public override string? ToString() => Content;

        public static implicit operator NullableQuote(string? content) =>
            content == null ? default : DoubleQuoted(content);

        public static implicit operator string?(NullableQuote quote) =>
            quote.Content;
    }

    internal readonly struct Quote
    {
        public static Quote DoubleQuoted(string message) =>
            new($"\"{message}\"");

        public static Quote SingleQuoted(string message) =>
            new($"'{message}'");

        public string Content => _content ?? throw new InvalidOperationException("Quote is not set");

        private readonly string? _content;

        public Quote(string content) =>
            _content = content ?? throw new ArgumentNullException(nameof(content));

        public override string? ToString() => Content;

        public static implicit operator Quote(string content) =>
            DoubleQuoted(content);

        public static implicit operator string(Quote quote) =>
            quote.Content;
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) {
            return;
        }

        if (disposing) {
        }

        _isDisposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
