using System.Diagnostics.CodeAnalysis;
using Vernuntii.Diagnostics;
using Vernuntii.Text;

namespace Vernuntii.Git.Commands
{
    /// <summary>
    /// The git command with limited capabilities.
    /// </summary>
    public class GitCommand : IGitCommand
    {
        private bool _isDisposed;
        private Lazy<LibGit2Command> _libGit2;

        /// <inheritdoc/>
        public string WorkingTreeDirectory { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="workingTreeDirectory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public GitCommand(string workingTreeDirectory)
        {
            WorkingTreeDirectory = workingTreeDirectory ?? throw new ArgumentNullException(nameof(workingTreeDirectory));
            _libGit2 = new Lazy<LibGit2Command>(() => new LibGit2Command(workingTreeDirectory));
        }

        private GitProcessStartInfo CreateStartInfo(string? args) => new GitProcessStartInfo(args, WorkingTreeDirectory);

        /// <summary>
        /// Executes the git command.
        /// </summary>
        /// <param name="args">The arguments next to the git-command.</param>
        /// <returns>The exit code.</returns>
        protected int ExecuteCommand(string args) => SimpleProcess.StartThenWaitForExit(CreateStartInfo(args));

        /// <summary>
        /// Executes the git command.
        /// </summary>
        /// <param name="args">The arguments next to the git-command.</param>
        /// <param name="expectedExitCode">The expected exit code.</param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void ExecuteCommandThenSucceed(string args, int expectedExitCode = 0)
        {
            var actualExitCode = ExecuteCommand(args);

            if (actualExitCode != expectedExitCode) {
                throw new InvalidOperationException($"Git returned unexpected exit code (Expected = expectedExitCode, Actual = {actualExitCode}, Args = \"{args}\")");
            }
        }

        /// <summary>
        /// Executes the git command and then reads the output.
        /// </summary>
        /// <param name="args">The arguments next to the git-command.</param>
        /// <returns>The read output.</returns>
        protected string ExecuteCommandThenReadOutput(string? args) =>
            SimpleProcess.StartThenWaitForExitThenReadOutput(CreateStartInfo(args), shouldThrowOnNonZeroCode: true).TrimEnd();

        /// <summary>
        /// Executes the command and then reads the outputted lines.
        /// </summary>
        /// <param name="args">The arguments next to the git-command.</param>
        /// <returns>The outputted lines.</returns>
        protected List<string> ExecuteCommandThenReadLines(string? args) => SimpleProcess
            .StartThenWaitForExitThenReadOutput(
                CreateStartInfo(args),
                shouldThrowOnNonZeroCode: true)
            .Split("\n")
            .ToList();

        /// <inheritdoc/>
        public bool IsHeadDetached() => _libGit2.Value.IsHeadDetached();

        /// <inheritdoc/>
        public string GetGitDirectory() => ExecuteCommandThenReadOutput("rev-parse --absolute-git-dir");

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

                    string[] strArray = line.Split(' ');
                    string referenceName = strArray[1];

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

                int firstSpace = line.IndexOf(' ', StringComparison.Ordinal);

                if (firstSpace == -1) {
                    break;
                }

                string sha = line[..firstSpace];
                string subject = line[(firstSpace + 1)..];
                yield return new Commit(sha, subject);
            }

            string BuildArguments()
            {
                CultureStringBuilder stringBuilder = CultureStringBuilder.Invariant();
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

        private class GitProcessStartInfo : SimpleProcessStartInfo
        {
            private const string GitCommandName = "git";

            public GitProcessStartInfo(string? args = null, string? workingDirectory = null)
              : base(GitCommandName, args, workingDirectory)
            {
            }
        }

        internal readonly struct NullableQuote
        {
            public static NullableQuote DoubleQuoted(string content) =>
                new NullableQuote($"\"{content}\"");

            public static NullableQuote SingleQuoted(string content) =>
                new NullableQuote($"'{content}'");

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
                new Quote($"\"{message}\"");

            public static Quote SingleQuoted(string message) =>
                new Quote($"'{message}'");

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
}
