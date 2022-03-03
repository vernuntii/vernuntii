using Vernuntii.Git;
using Vernuntii.MessagesProviders;

namespace Vernuntii.Extensions.Configurers
{
    /// <summary>
    /// Provides configuration capabilities for pre-release.
    /// </summary>
    public interface ISinceCommitConfigurer
    {
        /// <summary>
        /// Service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
        /// <summary>
        /// The repository from service provider.
        /// </summary>
        IRepository Repository { get; }

        /// <summary>
        /// Sets <see cref="CommitVersionFindingOptions.SinceCommit"/>.
        /// </summary>
        /// <param name="sinceCommit"></param>
        void SetVersionFindingSinceCommit(string? sinceCommit);

        /// <summary>
        /// Sets <see cref="GitCommitMessagesProviderOptions.SinceCommit"/>.
        /// If <see cref="CommitVersionFindingCache"/> is registered and
        /// <see cref="CommitVersionFindingCache.CommitVersion"/> is not null
        /// <see cref="GitCommitMessagesProviderOptions.SinceCommit"/> is
        /// set instead to <see cref="CommitVersionFindingCache.CommitVersion"/>
        /// and its <see cref="CommitVersion.CommitSha"/> .
        /// </summary>
        /// <param name="sinceCommit"></param>
        void SetMessageReadingSinceCommit(string? sinceCommit);
    }
}
