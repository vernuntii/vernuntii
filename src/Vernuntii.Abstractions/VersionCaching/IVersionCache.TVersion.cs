using Vernuntii.SemVer;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// The facts of produced semantic version.
    /// </summary>
    public interface IVersionCache<out TVersion> : IExpirableVersionCache
        where TVersion : ISemanticVersion
    {
        /// <summary>
        /// The version.
        /// </summary>
        TVersion Version { get; }

        /// <summary>
        /// The branch name.
        /// </summary>
        string BranchName { get; }
    }
}
