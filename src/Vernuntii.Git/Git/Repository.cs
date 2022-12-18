using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vernuntii.Caching;
using Vernuntii.Git.Commands;
using Vernuntii.SemVer;

namespace Vernuntii.Git
{
    /// <inheritdoc/>
    public class Repository : IRepository
    {
        internal static Repository Create(RepositoryOptions repositoryOptions, IGitCommand gitCommand, ILogger<Repository> logger) =>
            new(repositoryOptions, gitCommand, DefaultMemoryCacheFactory.Default.Create(), logger);

        internal static Repository Create(RepositoryOptions repositoryOptions, string workingTreeDirectory, ILogger<Repository> logger) =>
            new(repositoryOptions, new GitCommand(workingTreeDirectory), DefaultMemoryCacheFactory.Default.Create(), logger);

        internal static Repository Create(RepositoryOptions repositoryOptions, GitCommandOptions gitCommandOptions, ILogger<Repository> logger) =>
            new(repositoryOptions, new GitCommand(gitCommandOptions.GitWorkingTreeDirectory), DefaultMemoryCacheFactory.Default.Create(), logger);

        internal static Repository Create(IGitCommand gitCommand, ILogger<Repository> logger) =>
            Create(RepositoryOptions.s_default, gitCommand, logger);

        internal static Repository Create(string workingTreeDirectory, ILogger<Repository> logger) =>
            Create(RepositoryOptions.s_default, workingTreeDirectory, logger);

        internal static Repository Create(GitCommandOptions gitCommandOptions, ILogger<Repository> logger) =>
            Create(RepositoryOptions.s_default, gitCommandOptions, logger);

        /// <inheritdoc/>
        public IBranches Branches => _branches ??= LoadBranches();

        private CachingGitCommand _gitCommand { get; }
        private readonly IMemoryCache _memoryCache;
        private readonly RepositoryOptions _repositoryOptions;
        private readonly ILogger<Repository> _logger;
        private Branches? _branches;
        private readonly HashSet<CommitVersion> _commitVersions;
        private readonly Action<ILogger, string, Exception?> _logBranches;

        /// <summary>
        /// Creates an instance of <see cref="Repository"/>.
        /// </summary>
        /// <param name="repositoryOptions"></param>
        /// <param name="gitCommand"></param>
        /// <param name="memoryCache"></param>
        /// <param name="logger"></param>
        internal Repository(RepositoryOptions repositoryOptions, IGitCommand gitCommand, IMemoryCache memoryCache, ILogger<Repository> logger)
        {
            _repositoryOptions = repositoryOptions;
            _gitCommand = new CachingGitCommand(gitCommand, memoryCache);
            _memoryCache = memoryCache;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logBranches = LoggerMessage.Define<string>(
                LogLevel.Debug,
                new EventId(1),
                "Loaded branches: {Branches}");

            _commitVersions = new HashSet<CommitVersion>(SemanticVersionComparer.VersionReleaseBuild);
            ValidateRepository();
        }

        /// <summary>
        /// Creates an instance of <see cref="Repository"/>.
        /// </summary>
        /// <param name="repositoryOptions"></param>
        /// <param name="gitCommand"></param>
        /// <param name="memoryCacheFactory"></param>
        /// <param name="logger"></param>
        public Repository(IOptionsSnapshot<RepositoryOptions> repositoryOptions, IGitCommand gitCommand, IMemoryCacheFactory memoryCacheFactory, ILogger<Repository> logger)
            : this(repositoryOptions.Value, gitCommand, memoryCacheFactory.Create(), logger)
        {
        }

        /// <summary>
        /// Validates, whether the repository is not shallowed.
        /// </summary>
        /// <exception cref="ShallowRepositoryException"></exception>
        protected virtual void ValidateRepository()
        {
            if (!_repositoryOptions.AllowShallow && _gitCommand.IsShallow()) {
                throw new ShallowRepositoryException("Repository is not allowed to be shallow to prevent misbehavior") {
                    GitDirectory = _gitCommand.WorkingTreeDirectory
                };
            }
        }

        private Branches LoadBranches()
        {
            var branches = _gitCommand.GetBranches().ToList();
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
            _gitCommand.GetGitDirectory();

        /// <inheritdoc/>
        public IEnumerable<ICommitTag> GetCommitTags() =>
            _gitCommand.GetCommitTags();

        /// <inheritdoc/>
        public IReadOnlyCollection<ICommitVersion> GetCommitVersions()
        {
            if (!_memoryCache.IsCached(CachingGitCommand.GetCommitTagsCacheKey)) {
                foreach (var commitTag in GetCommitTags()) {
                    if (SemanticVersion.TryParse(commitTag.TagName, out var version)) {
                        _commitVersions.Add(new CommitVersion(commitTag.CommitSha, version));
                    }
                }
            }

            return _commitVersions;
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
