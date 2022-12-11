using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// A calcutor for next semantic version.
    /// </summary>
    public interface IVersionIncrementer
    {
        /// <summary>
        /// Calculates the next version.
        /// </summary>
        /// <returns>The next semantic version</returns>
        ISemanticVersion IncrementVersion(VersionIncrementationOptions options);
    }
}
