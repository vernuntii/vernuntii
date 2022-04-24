using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// Represents a fixed calculation of the next version.
    /// </summary>
    public interface ISemanticVersionCalculation
    {
        /// <summary>
        /// Gets the version of current calculation.
        /// </summary>
        SemanticVersion GetVersion();
    }
}
