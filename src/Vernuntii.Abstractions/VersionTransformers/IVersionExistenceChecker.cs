using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// An adapter that checks for existence of a version.
    /// </summary>
    public interface IVersionExistenceChecker
    {
        /// <summary>
        /// Checks if version exists.
        /// </summary>
        /// <param name="version"></param>
        ISemanticVersion IsVersionExisting(ISemanticVersion version);
    }
}
