using Microsoft.Extensions.Logging;
using Vernuntii.Git.Commands;
using Teronis.IO;
using Vernuntii.SemVer;
using Vernuntii.Caching;

namespace Vernuntii.Git
{
    /// <inheritdoc/>
    public class Repository : IRepository
    {
        /// <inheritdoc/>
        public IBranches Branches => _branches ??= LoadBranches();

        internal CachingGitCommand GitCommand => _gitCommand ??= CreateCommand();

        private bool _areCommitVersionsInitialized;
        private RepositoryOptions _options;
        private readonly IMemoryCacheFactory _memoryCacheFactory;
        private readonly ILogger<Repository> _logger;
        private CachingGitCommand? _gitCommand;
        private Branches? _branches;
        private readonly HashSet<CommitVersion> _commitVersions;
        private readonly Action<ILogger, string, Exception?> _logBranches;

        /// <summary>
        /// Creates an instance of <see cref="Repository"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public Repository(RepositoryOptions options, ILogger<Repository> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _memoryCacheFactory = DefaultMemoryCacheFactory.Default;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logBranches = LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1),
                "Loaded branches: {Branches}");

            _commitVersions = new HashSet<CommitVersion>(SemanticVersionComparer.VersionReleaseBuild);
        }

        internal void UnsetBranches() => _branches = null;

        internal void UnsetCommitVersions()
        {
            _commitVersions.Clear();
            _areCommitVersionsInitialized = false;
        }

        internal virtual Func<string, IGitCommand> CreateCommandFactory() => gitWorkingDirectory => {
            var gitCommand = _options.GitCommandFactory.CreateCommand(gitWorkingDirectory)
                ?? throw new InvalidOperationException("Git command factory produced null");

            if (gitCommand.IsShallow()) {
                throw new ShallowRepositoryException("Repository is not allowed to be shallow to prevent misbehavior") {
                    GitDirectory = gitWorkingDirectory
                };
            }

            return gitCommand;
        };

        /// <summary>
        /// Finds top-level working tree directory that contains the .git-directory or the .git-file.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual string GetWorkingTreeDirectory() =>
            _options.GitDirectoryResolver.ResolveWorkingTreeDirectory(_options.GitWorkingTreeDirectory);

        private CachingGitCommand CreateCommand()
        {
            var commandFactory = CreateCommandFactory();
            var gitWorkingDirectory = GetWorkingTreeDirectory();
            return new CachingGitCommand(commandFactory(gitWorkingDirectory), _memoryCacheFactory);
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

        /// <inheritdoc/>
        public string GetGitDirectory() =>
            GitCommand.GetGitDirectory();

        /// <inheritdoc/>
        public IEnumerable<ICommitTag> GetCommitTags() =>
            GitCommand.GetCommitTags();

        /// <inheritdoc/>
        public IReadOnlyCollection<ICommitVersion> GetCommitVersions()
        {
            if (!_areCommitVersionsInitialized) {
                foreach (var commitTag in GetCommitTags()) {
                    if (SemanticVersion.TryParse(commitTag.TagName, out var version)) {
                        _commitVersions.Add(new CommitVersion(version, commitTag.CommitSha));
                    }
                }

                _areCommitVersionsInitialized = true;
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

        /// <inheritdoc/>
        public string? ExpandBranchName(string? branchName)
        {
            if (GitCommand.TryResolveReference(branchName, ShowRefLimit.Heads, out var reference)) {
                return reference.ReferenceName;
            }

            return null;
        }

        /// <summary>
        /// Unsets cache that will lead to reload some data on request.
        /// </summary>
        public void UnsetCache()
        {
            _gitCommand?.UnsetCache();
            UnsetBranches();
            UnsetCommitVersions();
        }
    }
}
