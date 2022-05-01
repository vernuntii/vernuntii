using Microsoft.Extensions.Logging;
using Vernuntii.Git.Command;

namespace Vernuntii.Git
{
    internal class TempoararyRepository : Repository, IDisposable
    {
        internal new TestingGitCommand GitCommand => (TestingGitCommand)base.GitCommand;

        private readonly TemporaryRepositoryOptions _options;
        private bool _isDisposed;

        public TempoararyRepository(TemporaryRepositoryOptions options, ILogger<Repository> logger)
            : base(options.RepositoryOptions, logger)
        {
            if (options.DeleteOnDispose
                && options.DeleteOnlyTempDirectory
                && !Path.IsPathFullyQualified(options.RepositoryOptions.GitDirectory)) {
                throw new ArgumentException("Git directory must be fully qualified");
            }

            _options = options;
        }

        public TempoararyRepository(ILogger<Repository> logger)
            : this(new TemporaryRepositoryOptions(), logger)
        {
        }

        internal override Func<string, GitCommand> CreateCommandFactory()
        {
            var gitDirectory = _options.RepositoryOptions.GitDirectory;
            Directory.CreateDirectory(_options.RepositoryOptions.GitDirectory);
            var gitCommand = new TestingGitCommand(gitDirectory);
            gitCommand.Init();
            gitCommand.SetConfig("user.name", "Vernuntii");
            gitCommand.SetConfig("user.email", "vernuntii@vernuntii.dev");
            return _ => gitCommand;
        }

        public void Init() =>
            GitCommand.Init();

        public void SetConfig(string name, string value) =>
            GitCommand.SetConfig(name: name, value: value);

        public void Commit(string? message = null, bool allowEmpty = false, bool allowEmptyMessage = false) =>
            GitCommand.Commit(message: message, allowEmpty: allowEmpty, allowEmptyMessage: allowEmptyMessage);

        public void TagLightweight(string tagName, string? commit = default) =>
            GitCommand.TagLightweight(tagName: tagName, commit: commit);

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) {
                return;
            }


            if (disposing && _options.DeleteOnDispose) {
                var gitDirectory = _options.RepositoryOptions.GitDirectory;

                if (Directory.Exists(gitDirectory)
                    && (!_options.DeleteOnlyTempDirectory || ContainsBasePath(gitDirectory, Path.GetTempPath()))) {
                    foreach (var filePath in Directory.EnumerateFiles(gitDirectory, "*", SearchOption.AllDirectories)) {
                        File.SetAttributes(filePath, FileAttributes.Normal);
                    }

                    Directory.Delete(_options.RepositoryOptions.GitDirectory, recursive: true);
                }

                static bool ContainsBasePath(string subPath, string basePath)
                {
                    var relativePath = Path.GetRelativePath(basePath, subPath);
                    return !relativePath.StartsWith('.') && !Path.IsPathRooted(relativePath);
                }
            }

            _isDisposed = true;
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
