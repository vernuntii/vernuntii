using Microsoft.Extensions.Options;
using Vernuntii.Git;

namespace Vernuntii.Extensions
{
    /// <summary>
    /// Cache for <see cref="CommitVersionFindingCache"/>.
    /// </summary>
    internal class CommitVersionFindingCache
    {
        public CommitVersionFindingCache(ICommitVersionFinder commitVersionFinder, IOptions<CommitVersionFindingOptions> findingOptions)
        {
            var commitVersion = commitVersionFinder.FindCommitVersion(findingOptions.Value);
            bool commitVersionCoreAlreadyReleased;

            if (commitVersion != null) {
                commitVersionCoreAlreadyReleased = commitVersionFinder.IsVersionCoreReleased(commitVersion);
            } else {
                commitVersionCoreAlreadyReleased = false;
            }

            CommitVersion = commitVersion;
            CommitVersionCoreAlreadyReleased = commitVersionCoreAlreadyReleased;
        }

        /// <summary>
        /// The found commit version.
        /// </summary>
        public IPositonalCommitVersion? CommitVersion { get; }

        /// <summary>
        /// True means that the version core of <see cref="CommitVersion"/>
        /// has not been already used.
        /// </summary>
        public bool CommitVersionCoreAlreadyReleased { get; }
    }
}
