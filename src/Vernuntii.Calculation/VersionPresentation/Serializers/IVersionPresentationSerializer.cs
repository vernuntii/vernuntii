namespace Vernuntii.VersionPresentation.Serializers
{
    internal interface IVersionPresentationSerializer
    {
        string? SerializeSemanticVersion(
            object versionPresentation,
            VersionPresentationKind presentationKind,
            VersionPresentationPart presentationParts);
    }
}
