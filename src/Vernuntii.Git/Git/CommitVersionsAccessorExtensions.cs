using Vernuntii.SemVer;
using Vernuntii.Extensions;

namespace Vernuntii.Git
{
    /// <summary>
    /// Extension methods for <see cref="IRepository"/>
    /// </summary>
    public static class CommitVersionsAccessorExtensions
    {
        /// <summary>
        /// Checks if version exists.
        /// </summary>
        /// <param name="commitVersionAccessor"></param>
        /// <param name="version"></param>
        public static bool HasCommitVersion(this ICommitVersionsAccessor commitVersionAccessor, SemanticVersion version) =>
            commitVersionAccessor.GetCommitVersions().BinarySearch(version, SemanticVersionComparer.VersionReleaseBuild) != -1;
    }
}
