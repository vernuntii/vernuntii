namespace Vernuntii.VersionPersistence.Presentation.Serializers
{
    internal interface IVersionPresentationSerializer
    {
        string SerializeSemanticVersion(
            IVersionCache versionCache,
            VersionPresentationKind presentationKind,
            VersionPresentationParts presentationParts);
    }
}
