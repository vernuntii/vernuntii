namespace Vernuntii.SemVer
{
    /// <summary>
    /// Defines methods to compare two semantic versions.
    /// </summary>
    public interface ISemanticVersionComparer : IComparer<ISemanticVersion>, IEqualityComparer<ISemanticVersion>
    {
    }
}
