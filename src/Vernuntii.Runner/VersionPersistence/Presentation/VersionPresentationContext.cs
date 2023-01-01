namespace Vernuntii.VersionPersistence.Presentation;

public sealed class VersionPresentationContext
{
    /// <summary>
    /// The parts, that are allowed to be visualized and choosable by the user.
    /// </summary>
    public HashSet<VersionCachePart> PresentableParts { get; } = new();
}
