using Microsoft.Extensions.Logging;
using Vernuntii.Caching;
using Vernuntii.Git.Commands;

namespace Vernuntii.Git
{
    internal class TemporaryRepository : Repository, IDisposable
    {
        private static IGitCommand CreateGitCommand(string workingTreeDirectory, out TestingGitCommand testingGitCommand) =>
            testingGitCommand = new TestingGitCommand(workingTreeDirectory);

        private static IMemoryCache CreateMemoryCache(out IMemoryCache memoryCache) =>
            memoryCache = new DefaultMemoryCache();

        internal TestingGitCommand GitCommand { get; }

        private readonly TemporaryRepositoryOptions _options;
        private readonly IMemoryCache _memoryCache;
        private bool _isDisposed;

        public TemporaryRepository(TemporaryRepositoryOptions options, ILogger<TemporaryRepository> logger) : base(
            RepositoryOptions.s_default,
            CreateGitCommand(options.CommandOptions.GitWorkingTreeDirectory, out var testingGitCommand),
            CreateMemoryCache(out var memoryCache),
            logger)
        {
            GitCommand = testingGitCommand;
            _memoryCache = memoryCache;

            if (options.DeleteOnDispose
                && options.DeleteOnlyTempDirectory
                && !Path.IsPathFullyQualified(options.CommandOptions.GitWorkingTreeDirectory)) {
                throw new ArgumentException("Git directory must be fully qualified");
            }

            _options = options;
            InitializeRepository();
        }

        public TemporaryRepository(TemporaryRepositoryOptions options)
            : this(options, DefaultTemporaryRepositoryLogger)
        {
        }

        public TemporaryRepository(ILogger<TemporaryRepository> logger)
            : this(new TemporaryRepositoryOptions(), logger)
        {
        }

        internal TemporaryRepository()
            : this(DefaultTemporaryRepositoryLogger)
        {
        }

        /// <summary>
        /// We NOOP base implementation of <see cref="Repository.ValidateRepository"/> to call it once from derived class.
        /// </summary>
        protected sealed override void ValidateRepository()
        {
        }

        private void InitializeRepository()
        {
            Directory.CreateDirectory(_options.CommandOptions.GitWorkingTreeDirectory);

            try {
                var cloneOptions = _options.CloneOptions;

                if (cloneOptions != null) {
                    GitCommand.Clone(cloneOptions.SourceUrl(), cloneOptions.Depth);
                } else {
                    GitCommand.Init();
                }

                GitCommand.SetConfig("user.name", "Vernuntii");
                GitCommand.SetConfig("user.email", "vernuntii@vernuntii.dev");
                base.ValidateRepository();
            } catch {
                Dispose();
                throw;
            }
        }

        public void SetConfig(string name, string value) =>
            GitCommand.SetConfig(name: name, value: value);

        public void Commit(string? message = null, bool allowEmpty = false, bool allowEmptyMessage = false) =>
            GitCommand.Commit(
                message: message,
                allowEmpty: allowEmpty,
                allowEmptyMessage: allowEmptyMessage);

        public void TagLightweight(string tagName, string? commit = default) =>
            GitCommand.TagLightweight(tagName, commit: commit);

        public void Checkout(string branchName) =>
            GitCommand.Checkout(branchName);

        public IReadOnlyCollection<ICommitVersion> GetCommitVersions(bool unsetCache)
        {
            if (unsetCache) {
                _memoryCache.UnsetCache(CachingGitCommand.GetCommitTagsCacheKey);
            }

            return GetCommitVersions();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) {
                return;
            }

            if (disposing && _options.DeleteOnDispose) {
                var gitDirectory = _options.CommandOptions.GitWorkingTreeDirectory;

                if (Directory.Exists(gitDirectory)
                    && (!_options.DeleteOnlyTempDirectory || ContainsBasePath(gitDirectory, Path.GetTempPath()))) {
                    foreach (var filePath in Directory.EnumerateFiles(gitDirectory, "*", SearchOption.AllDirectories)) {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }

                    Directory.Delete(_options.CommandOptions.GitWorkingTreeDirectory, recursive: true);
                }

                static bool ContainsBasePath(string subPath, string basePath)
                {
                    var relativePath = Path.GetRelativePath(basePath, subPath);
                    return !relativePath.StartsWith('.') && !Path.IsPathRooted(relativePath);
                }
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
