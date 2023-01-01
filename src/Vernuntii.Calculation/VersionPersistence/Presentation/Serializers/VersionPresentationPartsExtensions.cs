namespace Vernuntii.VersionPersistence.Presentation.Serializers;

internal static class VersionPresentationPartsExtensions
{
    /// <summary>
    /// Gets the single presentation part, otherwise an exception is thrown.
    /// </summary>
    /// <param name="_parts"></param>
    /// <exception cref="NotSupportedException"></exception>
    public static VersionCachePart AssertSingleDueToValueKind(this VersionPresentationParts _parts) => _parts.Count == 1
        ? _parts.Single()
        : throw new VersionPresentationKindException($"Version presentation parts consists of zero or more than one parts, but the presentation '{nameof(VersionPresentationKind.Value)}' kind supports only one presentation part");
}
