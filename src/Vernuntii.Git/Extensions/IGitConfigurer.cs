using Vernuntii.Git;
using Vernuntii.MessagesProviders;
using Vernuntii.VersionTransformers;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Provides configuration capabilities for pre-release.
    /// </summary>
    public interface IGitConfigurer
    {
        /// <summary>
        /// Service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Sets <see cref="FoundCommitVersionOptions.SinceCommit"/> and
        /// sets <see cref="GitCommitMessagesProviderOptions.SinceCommit"/>.
        /// If <see cref="FoundCommitVersion"/> is registered and
        /// <see cref="FoundCommitVersion.CommitVersion"/> is not null
        /// <see cref="GitCommitMessagesProviderOptions.SinceCommit"/> is
        /// set instead to <see cref="FoundCommitVersion.CommitVersion"/>
        /// and its <see cref="CommitVersion.CommitSha"/>.
        /// </summary>
        /// <param name="sinceCommit"></param>
        void SetSinceCommit(string? sinceCommit);

        /// <summary>
        /// Sets <see cref="FoundCommitVersionOptions.BranchName"/> and
        /// sets <see cref="GitCommitMessagesProviderOptions.BranchName"/>.
        /// </summary>
        /// <param name="branchName"></param>
        void SetBranch(string? branchName);

        /// <summary>
        /// Sets <see cref="FoundCommitVersionOptions.PreRelease"/>.
        /// </summary>
        /// <param name="preRelease"></param>
        void SetSearchPreRelease(string? preRelease);

        /// <summary>
        /// Sets <see cref="VersionIncrementationOptions.PostTransformer"/> to
        /// <see cref="PreReleaseTransformer"/> with <paramref name="preRelease"/>.
        /// </summary>
        /// <param name="preRelease"></param>
        void SetPostPreRelease(string? preRelease);
    }
}
