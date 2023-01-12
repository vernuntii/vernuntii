namespace Vernuntii.SemVer;

/// <summary>
/// Provides an instance of <see cref="ISemanticVersionContext"/>.
/// </summary>
public interface ISemanticVersionContextProvider
{
    /// <summary>
    /// Semantic version parser.
    /// </summary>
    ISemanticVersionContext Context { get; }
}
