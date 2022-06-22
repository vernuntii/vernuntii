using Microsoft.Extensions.Logging;
using Vernuntii.Git.Command;

namespace Vernuntii.Git
{
    internal class TemporaryRepository : Repository, IDisposable
    {
        internal new TestingGitCommand GitCommand => (TestingGitCommand)base.GitCommand.UnderlyingCommand;

        private readonly TemporaryRepositoryOptions _options;
        private bool _isDisposed;

        public TemporaryRepository(TemporaryRepositoryOptions options, ILogger<TemporaryRepository> logger)
            : base(options.RepositoryOptions, logger)
        {
            if (options.DeleteOnDispose
                && options.DeleteOnlyTempDirectory
                && !Path.IsPathFullyQualified(options.RepositoryOptions.GitWorkingTreeDirectory)) {
                throw new ArgumentException("Git directory must be fully qualified");
            }

            _options = options;
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

        internal override Func<string, GitCommand> CreateCommandFactory()
        {
            var gitDirectory = _options.RepositoryOptions.GitWorkingTreeDirectory;
            Directory.CreateDirectory(_options.RepositoryOptions.GitWorkingTreeDirectory);
            var gitCommand = new TestingGitCommand(gitDirectory);
            var cloneOptions = _options.CloneOptions;

            if (cloneOptions != null) {
                gitCommand.Clone(cloneOptions.SourceUrl(), cloneOptions.Depth);
            } else {
                gitCommand.Init();
            }

            gitCommand.SetConfig("user.name", "Vernuntii");
            gitCommand.SetConfig("user.email", "vernuntii@vernuntii.dev");
            return _ => gitCommand;
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
                base.GitCommand.UnsetCommitTagsCache();
                UnsetCommitVersions();
            }

            return GetCommitVersions();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) {
                return;
            }

            if (disposing && _options.DeleteOnDispose) {
                var gitDirectory = _options.RepositoryOptions.GitWorkingTreeDirectory;

                if (Directory.Exists(gitDirectory)
                    && (!_options.DeleteOnlyTempDirectory || ContainsBasePath(gitDirectory, Path.GetTempPath()))) {
                    foreach (var filePath in Directory.EnumerateFiles(gitDirectory, "*", SearchOption.AllDirectories)) {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }

                    Directory.Delete(_options.RepositoryOptions.GitWorkingTreeDirectory, recursive: true);
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
