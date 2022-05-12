using Vernuntii.SemVer;

namespace Vernuntii.HeightConventions.Transformation
{
    /// <summary>
    /// An adapter that checks for existence of a version.
    /// </summary>
    public interface ISemanticVersionExistenceChecker
    {
        ISemanticVersion IsVersionExisting(ISemanticVersion version);
    }
}
