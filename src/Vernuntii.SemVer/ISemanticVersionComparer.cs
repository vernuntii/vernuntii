namespace Vernuntii
{
    /// <summary>
    /// Defines methods to compare two semantic versions.
    /// </summary>
    public interface ISemanticVersionComparer : IComparer<SemanticVersion>, IEqualityComparer<SemanticVersion>
    {
    }
}
