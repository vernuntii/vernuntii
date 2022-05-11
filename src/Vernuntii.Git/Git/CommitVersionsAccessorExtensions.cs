using Vernuntii.SemVer;

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
        public static bool HasCommitVersion(this ICommitVersionsAccessor commitVersionAccessor, ISemanticVersion version) =>
            commitVersionAccessor.GetCommitVersions().Contains(version);

        /// <summary>
        /// Checks if version exists.
        /// </summary>
        /// <param name="commitVersionAccessor"></param>
        /// <param name="version"></param>
        /// <param name="comparer"></param>
        public static bool HasCommitVersion(this ICommitVersionsAccessor commitVersionAccessor, ISemanticVersion version, IEqualityComparer<ISemanticVersion> comparer) =>
            commitVersionAccessor.GetCommitVersions().Contains(version, comparer);
    }
}
