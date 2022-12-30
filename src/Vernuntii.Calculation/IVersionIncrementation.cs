using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// Represents a fixed incrementation of a version.
    /// </summary>
    public interface IVersionIncrementation
    {
        /// <summary>
        /// Gets the version of current calculation.
        /// </summary>
        ISemanticVersion GetIncrementedVersion();
    }
}
