using Microsoft.Extensions.Configuration;
using Vernuntii.Git;

namespace Vernuntii.Configuration.Shadowing
{
    internal class GitDirectoryCorrectionConfigurationProvider : ConfigurationProvider
    {
        private readonly DirectoryInfo _directory;
        private readonly IConfigurationProvider _originConfigurationProvider;

        public GitDirectoryCorrectionConfigurationProvider(DirectoryInfo directory, IConfigurationProvider originConfigurationProvider)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _originConfigurationProvider = originConfigurationProvider ?? throw new ArgumentNullException(nameof(originConfigurationProvider));
        }

        private bool IsGitDirectoryShadowable(out string value)
        {
            _ = _originConfigurationProvider.TryGet(nameof(RepositoryOptions.GitDirectory), out value);

            if (string.IsNullOrEmpty(value) || !Path.IsPathRooted(value)) {
                return true;
            }

            return false;
        }

        private bool IsGitDirectoryShadowable(string key, out string value)
        {
            if (key == nameof(RepositoryOptions.GitDirectory)) {
                return IsGitDirectoryShadowable(out value);
            }

            value = string.Empty;
            return false;
        }

        public override bool TryGet(string key, out string value)
        {
            if (IsGitDirectoryShadowable(key, out value)) {
                if (string.IsNullOrEmpty(value)) {
                    value = _directory.FullName;
                } else {
                    value = Path.GetFullPath(value, _directory.FullName);
                }

                return true;
            }

            value = string.Empty;
            return false;
        }

        public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string? parentPath)
        {
            if (parentPath == null && IsGitDirectoryShadowable(out _)) {
                return new List<string>(earlierKeys) {
                    nameof(RepositoryOptions.GitDirectory)
                };
            }

            return earlierKeys;
        }
    }
}
