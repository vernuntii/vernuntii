using Vernuntii.SemVer;

namespace Vernuntii
{
    /// <summary>
    /// A calcutor for next semantic version.
    /// </summary>
    public interface ISemanticVersionCalculator
    {
        /// <summary>
        /// Calculates the next version.
        /// </summary>
        /// <returns>The next semantic version</returns>
        ISemanticVersion CalculateVersion(SemanticVersionCalculationOptions options);
    }
}
