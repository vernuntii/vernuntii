using Microsoft.Extensions.Options;
using Vernuntii.Git;
using Vernuntii.MessagesProviders;

namespace Vernuntii.Extensions.Configurers
{
    internal class BranchNameConfigurer : GitConfigurer, IBranchNameConfigurer, IConfigureOptions<CommitVersionFindingOptions>, IConfigureOptions<GitCommitMessagesProviderOptions>
    {
        private bool _handleVersionFindingBranch;
        private string? _versionFindingBranch;
        private bool _handleMessageReadingBranch;
        private string? _messageReadingBranch;

        public BranchNameConfigurer(IServiceProvider serviceProvider, IRepository repository)
            : base(serviceProvider, repository)
        {
        }

        public void SetVersionFindingSinceCommit(string? branchName)
        {
            _handleVersionFindingBranch = true;
            _versionFindingBranch = branchName;
        }

        public void SetMessageReadingSinceCommit(string? branchName)
        {
            _handleMessageReadingBranch = true;
            _messageReadingBranch = branchName;
        }

        public void Configure(CommitVersionFindingOptions options)
        {
            if (_handleVersionFindingBranch) {
                options.BranchName = _versionFindingBranch;
            }
        }

        public void Configure(GitCommitMessagesProviderOptions options)
        {
            if (_handleMessageReadingBranch) {
                options.BranchName = _messageReadingBranch;
            }
        }
    }
}
