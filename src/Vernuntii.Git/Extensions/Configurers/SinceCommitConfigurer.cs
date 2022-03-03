using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Vernuntii.Git;
using Vernuntii.MessagesProviders;

namespace Vernuntii.Extensions.Configurers
{
    internal class SinceCommitConfigurer : GitConfigurer, ISinceCommitConfigurer, IConfigureOptions<CommitVersionFindingOptions>, IConfigureOptions<GitCommitMessagesProviderOptions>
    {
        private bool _handleVersionFindingSinceCommit;
        private string? _versionFindingSinceCommit;
        private bool _handleMessageReadingSinceCommit;
        private string? _messageReadingSinceCommit;

        public SinceCommitConfigurer(IServiceProvider serviceProvider, IRepository repository)
            : base(serviceProvider, repository)
        {
        }

        public void SetVersionFindingSinceCommit(string? sinceCommit)
        {
            _handleVersionFindingSinceCommit = true;
            _versionFindingSinceCommit = sinceCommit;
        }

        public void SetMessageReadingSinceCommit(string? sinceCommit)
        {
            _handleMessageReadingSinceCommit = true;
            _messageReadingSinceCommit = sinceCommit;
        }

        public void Configure(CommitVersionFindingOptions options)
        {
            if (_handleVersionFindingSinceCommit) {
                options.SinceCommit = _versionFindingSinceCommit;
            }
        }

        public void Configure(GitCommitMessagesProviderOptions options)
        {
            if (_handleMessageReadingSinceCommit) {
                options.SinceCommit = ServiceProvider.GetService<CommitVersionFindingCache>()?.CommitVersion?.CommitSha ?? _messageReadingSinceCommit;
            }
        }
    }
}
