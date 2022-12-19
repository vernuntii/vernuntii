using Microsoft.Extensions.Options;
using Vernuntii.MessagesProviders;
using Vernuntii.VersionTransformers;

namespace Vernuntii.Extensions
{
    internal class GitConfigurer :
        IGitConfigurer,
        IConfigureOptions<FoundCommitVersionOptions>,
        IConfigureOptions<GitCommitMessagesProviderOptions>,
        IConfigureOptions<VersionIncrementationOptions>
    {
        public IServiceProvider ServiceProvider { get; }

        private bool _handleSearchPreReleaseIdentifier;
        private string? _searchPreReleaseIdentifier;

        private bool _handlePostPreReleaseIdentifier;
        private string? _postPreReleaseIdentifier;

        private bool _handleVersionFindingSinceCommit;
        private string? _versionFindingSinceCommit;

        private bool _handleMessageReadingSinceCommit;
        private string? _messageReadingSinceCommit;

        private bool _handleVersionFindingBranch;
        private string? _versionFindingBranch;

        private bool _handleMessageReadingBranch;
        private string? _messageReadingBranch;

        public GitConfigurer(IServiceProvider serviceProvider) =>
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public void SetSearchPreRelease(string? preRelease)
        {
            _handleSearchPreReleaseIdentifier = true;
            _searchPreReleaseIdentifier = preRelease;
        }

        public void SetPostPreRelease(string? preRelease)
        {
            _handlePostPreReleaseIdentifier = true;
            _postPreReleaseIdentifier = preRelease;
        }

        public void SetSinceCommit(string? sinceCommit)
        {
            _handleVersionFindingSinceCommit = true;
            _versionFindingSinceCommit = sinceCommit;

            _handleMessageReadingSinceCommit = true;
            _messageReadingSinceCommit = sinceCommit;
        }

        public void SetBranch(string? branchName)
        {
            _handleVersionFindingBranch = true;
            _versionFindingBranch = branchName;

            _handleMessageReadingBranch = true;
            _messageReadingBranch = branchName;
        }

        public void Configure(GitCommitMessagesProviderOptions options)
        {
            if (_handleMessageReadingSinceCommit) {
                options.SinceCommit = _messageReadingSinceCommit;
            }

            if (_handleMessageReadingBranch) {
                options.BranchName = _messageReadingBranch;
            }
        }

        public void Configure(FoundCommitVersionOptions options)
        {
            if (_handleSearchPreReleaseIdentifier) {
                options.SearchPreRelease = _searchPreReleaseIdentifier;
            }

            if (_handlePostPreReleaseIdentifier) {
                options.IsPostVersionPreRelease = PreReleaseTransformer.IsPreRelease(_postPreReleaseIdentifier);
                options.PostPreRelease = _postPreReleaseIdentifier;
            }

            if (_handleVersionFindingSinceCommit) {
                options.SinceCommit = _versionFindingSinceCommit;
            }

            if (_handleVersionFindingBranch) {
                options.BranchName = _versionFindingBranch;
            }
        }

        public void Configure(VersionIncrementationOptions options)
        {
            if (_handlePostPreReleaseIdentifier) {
                options.PostTransformer = new PreReleaseTransformer(_postPreReleaseIdentifier);
            }
        }
    }
}
