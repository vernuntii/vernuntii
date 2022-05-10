using System.Diagnostics.CodeAnalysis;
using Vernuntii.Git.Diagnostics;

namespace Vernuntii.Git.Command
{
    internal class GitCommand
    {
        public string WorkingDirectory { get; }

        public GitCommand(string workingDirectory) =>
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));

        private GitProcessStartInfo CreateStartInfo(string? args) => new GitProcessStartInfo(args, WorkingDirectory);

        protected int ExecuteCommand(string args) => SimpleProcess.StartThenWaitForExit(CreateStartInfo(args));

        protected void ExecuteCommandThenSucceed(string args, int expectedExitCode = 0)
        {
            var actualExitCode = ExecuteCommand(args);

            if (actualExitCode != expectedExitCode) {
                throw new InvalidOperationException($"Git returned unexpected exit code: {expectedExitCode} (Actual = {actualExitCode}, Args = {args})");
            }
        }

        protected string ExecuteCommandThenReadOutput(string? args) =>
            SimpleProcess.StartThenWaitForExitThenReadOutput(CreateStartInfo(args), shouldThrowOnNonZeroCode: true).TrimEnd();

        protected List<string> ExecuteCommandThenReadLines(string? args) => SimpleProcess
            .StartThenWaitForExitThenReadOutput(
                CreateStartInfo(args),
                shouldThrowOnNonZeroCode: true)
            .Split("\n")
            .ToList();

        public bool IsHeadDetached() => ExecuteCommand("symbolic-ref -q HEAD") == 1;

        public string GetGitDirectory() => ExecuteCommandThenReadOutput("rev-parse --absolute-git-dir");

        public bool TryResolveReference(
          string? name,
          ShowRefLimit showRefLimit,
          [NotNullWhen(true)] out GitReference? reference)
        {
            var lines = ExecuteCommandThenReadLines(BuildArguments());
            lines.RemoveAll(string.IsNullOrWhiteSpace);

            if (lines.Count > 1) {
                throw new ArgumentException("Reference name \"" + name + "\" could not be resolved to single reference");
            }

            if (lines.Count == 1) {
                return GitReference.TryParse(lines[0], out reference);
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
        public string GetActiveBranchName() => ExecuteCommandThenReadOutput("rev-parse --symbolic-full-name HEAD");

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

        public IEnumerable<ICommitTag> GetCommitTags()
        {
            foreach (var line in ExecuteCommandThenReadLines("tag --format=\"%(if)%(*objectname)%(then)%(*objectname)%(else)%(objectname)%(end) %(refname:short)\"")) {
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                var strArray = line.Split(' ');
                yield return new CommitTag(strArray[0], strArray[1]);
            }
        }

        public IEnumerable<ICommit> GetCommits(
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

        public bool IsShallowRepository() =>
            ExecuteCommandThenReadOutput("rev-parse --is-shallow-repository").Equals("true", StringComparison.OrdinalIgnoreCase);

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
    }
}
