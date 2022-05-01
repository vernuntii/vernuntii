using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Vernuntii.Git.Diagnostics;

namespace Vernuntii.Git.Command
{
    internal class GitCommand
    {
        private readonly string _workingDirectory;

        public GitCommand(string workingDirectory) => 
            _workingDirectory = workingDirectory;

        private GitProcessStartInfo CreateStartInfo(string? args) => new GitProcessStartInfo(args, _workingDirectory);

        protected int ExecuteCommand(string args) => SimpleProcess.StartThenWaitForExit(CreateStartInfo(args));

        protected void ExecuteCommandThenSucceed(string args, int expectedExitCode = 0)
        {
            var actualExitCode = ExecuteCommand(args);

            if (actualExitCode != expectedExitCode) {
                throw new InvalidOperationException($"Git returned unexpected exit code: {expectedExitCode} (Actual = {actualExitCode})");
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
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(CultureInfo.InvariantCulture, $"show-ref {name}");

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
                    if (line.Contains("HEAD detached at head", StringComparison.Ordinal)) {
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
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(CultureInfo.InvariantCulture, $"log {branchName ?? "HEAD"}");

                if (!string.IsNullOrEmpty(sinceCommit)) {
                    stringBuilder.Append(CultureInfo.InvariantCulture, $" --not {sinceCommit}");
                }

                stringBuilder.Append(" --format=\"%H %s\"");

                if (reverse) {
                    stringBuilder.Append(" --reverse");
                }

                return stringBuilder.ToString();
            }
        }

        private class GitProcessStartInfo : SimpleProcessStartInfo
        {
            private const string GitCommandName = "git";

            public GitProcessStartInfo(string? args = null, string? workingDirectory = null)
              : base(GitCommandName, args, workingDirectory)
            {
            }
        }
    }
}
