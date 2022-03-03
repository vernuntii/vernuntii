using Vernuntii.Git;
using Vernuntii.MessagesProviders;

namespace Vernuntii.Extensions.Configurers
{
    /// <summary>
    /// Provides configuration capabilities for pre-release.
    /// </summary>
    public interface IBranchNameConfigurer
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
        /// Sets <see cref="CommitVersionFindingOptions.BranchName"/>.
        /// </summary>
        /// <param name="branchName"></param>
        void SetVersionFindingSinceCommit(string? branchName);

        /// <summary>
        /// Sets <see cref="GitCommitMessagesProviderOptions.BranchName"/>.
        /// </summary>
        /// <param name="branchName"></param>
        void SetMessageReadingSinceCommit(string? branchName);
    }
}
