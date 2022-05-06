using Microsoft.Extensions.Logging;
using Vernuntii.Git.Command;
using Teronis.Collections.Synchronization;
using Teronis.IO;
using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <inheritdoc/>
    public class Repository : IRepository
    {
        private const string GitFolderOrFileName = ".git";

        /// <inheritdoc/>
        public IBranches Branches => _branches ??= LoadBranches();

        internal GitCommand GitCommand => _gitCommand ??= CreateCommand();

        private bool areCommitVersionsInitialized;
        private RepositoryOptions _options;
        private readonly ILogger<Repository> _logger;
        private GitCommand? _gitCommand;
        private Branches? _branches;
        private readonly SynchronizableCollection<CommitVersion> _commitVersions;
        private readonly Action<ILogger, string, Exception?> _logGitDirectory;
        private readonly Action<ILogger, string, Exception?> _logBranches;

        /// <summary>
        /// Creates an instance of <see cref="Repository"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public Repository(RepositoryOptions options, ILogger<Repository> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logGitDirectory = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(2),
                @"Use repository directory: {GitDirectory}");

            _logBranches = LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1),
                "Loaded branches: {Branches}");

            _commitVersions = new SynchronizableCollection<CommitVersion>(SemanticVersionComparer.VersionReleaseBuild, descended: false);
        }

        internal virtual Func<string, GitCommand> CreateCommandFactory() => gitDirectory => {
            var gitCommand = new GitCommand(gitDirectory);

            if (gitCommand.IsShallowRepository()) {
                throw new ShallowRepositoryException("Repository is not allowed to be shallow to prevent misbehavior") {
                    GitDirectory = gitDirectory
                };
            }

            return gitCommand;
        };

        private GitCommand CreateCommand()
        {
            var commandFactory = CreateCommandFactory();
            var gitDirectory = GetGitRootDirectory();
            _logGitDirectory(_logger, gitDirectory, null);
            return commandFactory(gitDirectory);
        }

        private Branches LoadBranches()
        {
            var branches = GitCommand.GetBranches().ToList();
            LogBranches(branches);
            return new Branches(branches);
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
            }, new DirectoryInfo(_options.GitDirectory), includeBeginningDirectory: true)
                ?? throw new InvalidOperationException("Did not find a parent directory containing a .git-directory or .git-file")).FullName;

        /// <inheritdoc/>
        public string GetGitDirectory() =>
            GitCommand.GetGitDirectory();

        /// <inheritdoc/>
        public IEnumerable<ICommitTag> GetCommitTags() =>
            GitCommand.GetCommitTags();

        /// <inheritdoc/>
        public IReadOnlyList<ICommitVersion> GetCommitVersions()
        {
            if (!areCommitVersionsInitialized) {
                _commitVersions.SynchronizeCollection(ParseCommitTags(GetCommitTags()));
                areCommitVersionsInitialized = true;

                static IEnumerable<CommitVersion> ParseCommitTags(IEnumerable<ICommitTag> commitTags)
                {
                    foreach (var commitTag in commitTags) {
                        if (SemanticVersion.TryParse(commitTag.TagName, out var version)) {
                            yield return new CommitVersion(version, commitTag.CommitSha);
                        }
                    }
                }
            }

            return _commitVersions;
        }

        /// <inheritdoc/>
        public IEnumerable<ICommit> GetCommits(string? branchName = null, string? sinceCommit = null, bool reverse = false) =>
            GitCommand.GetCommits(branchName, sinceCommit, reverse);

        /// <inheritdoc/>
        public IBranch GetActiveBranch()
        {
            var activeBranchName = GitCommand.GetActiveBranchName();
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
            if (GitCommand.TryResolveReference(branchName, ShowRefLimit.Heads, out var reference)) {
                return reference.ReferenceName;
            }

            return null;
        }
    }
}
