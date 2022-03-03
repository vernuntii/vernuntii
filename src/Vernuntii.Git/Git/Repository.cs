using Microsoft.Extensions.Logging;
using Vernuntii.Git.Command;
using Teronis;
using Teronis.Collections.Synchronization;
using Teronis.IO;

namespace Vernuntii.Git
{
    /// <inheritdoc/>
    public class Repository : IRepository
    {
        private const string GitFolderOrFileName = ".git";

        /// <inheritdoc/>
        public Branches Branches => _lazyBranches.Value;

        private readonly RepositoryOptions _factoryOptions;
        private readonly ILogger<Repository> _logger;
        private readonly GitCommand _gitCommand;
        private readonly SlimLazy<Branches> _lazyBranches;
        private readonly SynchronizingCollection<ICommitTag, CommitVersion> _commitVersions;
        private readonly Action<ILogger, string, Exception?> _logGitDirectory;
        private readonly Action<ILogger, string, Exception?> _logBranches;

        /// <summary>
        /// Creates an instance of <see cref="Repository"/>.
        /// </summary>
        /// <param name="factoryOptions"></param>
        /// <param name="logger"></param>
        public Repository(RepositoryOptions factoryOptions, ILogger<Repository> logger)
        {
            _factoryOptions = factoryOptions ?? throw new ArgumentNullException(nameof(factoryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logGitDirectory = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(2),
                @"Use repository directory: {GitDirectory}");

            _logBranches = LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1),
                "Loaded branches: {Branches}");

            var gitDirectory = GetGitRootDirectory();
            _logGitDirectory(_logger, gitDirectory, null);
            _gitCommand = new GitCommand(gitDirectory);

            _lazyBranches = new SlimLazy<Branches>(() => {
                var branches = _gitCommand.GetBranches().ToList();
                LogBranches(branches);
                return new Branches(branches);
            });

            _commitVersions = new SynchronizingCollection<ICommitTag, CommitVersion>(commitTag => {
                if (SemanticVersion.TryParse(commitTag.TagName, out var version)) {
                    return new CommitVersion(version, commitTag.CommitSha);
                }

                return InvalidCommitVersion.Invalid;
            });
        }

        private void LogBranches(ICollection<IBranch> branches)
        {
            string branchesString;

            if (branches.Count == 0) {
                branchesString = "<none>";
            } else {
                branchesString = Environment.NewLine + string.Join(Environment.NewLine, branches.Select(branch => branch.LongBranchName));
            }

            _logBranches(_logger, branchesString, null);
        }

        /// <summary>
        /// Finds the root directory that contains the .git-directory or the .git-file.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual string GetGitRootDirectory() =>
            (DirectoryUtils.GetDirectoryOfPathAbove(directory => {
                var path = Path.Combine(directory.FullName, GitFolderOrFileName);
                return File.Exists(path) || Directory.Exists(path);
            }, new DirectoryInfo(_factoryOptions.GitDirectory), includeBeginningDirectory: true)
                ?? throw new InvalidOperationException("Did not find a parent directory containing a .git-directory or .git-file")).FullName;

        /// <inheritdoc/>
        public string GetGitDirectory() =>
            _gitCommand.GetGitDirectory();

        /// <inheritdoc/>
        public IEnumerable<ICommitTag> GetCommitTags() =>
            _gitCommand.GetCommitTags();

        /// <inheritdoc/>
        public IEnumerable<CommitVersion> GetCommitVersions()
        {
            _commitVersions.SynchronizeCollection(GetCommitTags());

            foreach (var commitVersion in _commitVersions.SubItems) {
                if (commitVersion.GetType() == InvalidCommitVersion.Type) {
                    continue;
                }

                yield return commitVersion;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ICommit> GetCommits(string? branchName = null, string? sinceCommit = null, bool reverse = false) =>
            _gitCommand.GetCommits(branchName, sinceCommit, reverse);

        /// <inheritdoc/>
        public IBranch GetActiveBranch()
        {
            var activeBranchName = _gitCommand.GetActiveBranchName();
            return Branches[activeBranchName] ?? throw new InvalidOperationException($"Active branch \"{activeBranchName}\" is not retrievable");
        }

        private record InvalidCommitVersion : CommitVersion
        {
            public static readonly InvalidCommitVersion Invalid = new InvalidCommitVersion();
            public static readonly Type Type = typeof(InvalidCommitVersion);

            private InvalidCommitVersion()
                : base(string.Empty)
            {
            }
        }

        /// <inheritdoc/>
        public string? ExpandBranchName(string? branchName)
        {
            if (_gitCommand.TryResolveReference(branchName, ShowRefLimit.Heads, out var reference)) {
                return reference.ReferenceName;
            }

            return null;
        }
    }
}
