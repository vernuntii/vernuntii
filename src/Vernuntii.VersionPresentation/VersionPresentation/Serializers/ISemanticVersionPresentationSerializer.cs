namespace Vernuntii.VersionPresentation.Serializers
{
    internal interface ISemanticVersionPresentationSerializer
    {
        string? SerializeSemanticVersion(
            object versionPresentation,
            SemanticVersionPresentationKind presentationKind,
            SemanticVersionPresentationPart presentationParts);
    }
}
