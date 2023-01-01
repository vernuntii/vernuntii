namespace Vernuntii.VersionPersistence.Presentation;

internal static class VersionCacheStringBuilderExtensions
{
    public static VersionPresentationParts UsePresentationParts(params VersionCachePart[] presentationParts) =>
        VersionPresentationParts.Of(presentationParts);
}
