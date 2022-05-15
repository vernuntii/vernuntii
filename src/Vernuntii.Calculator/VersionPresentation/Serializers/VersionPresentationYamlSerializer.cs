using YamlDotNet.Serialization;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class VersionPresentationYamlSerializer : IVersionPresentationSerializer
    {
        public readonly static VersionPresentationYamlSerializer Default = new VersionPresentationYamlSerializer();

        public string? SerializeSemanticVersion(
            object versionPresentation, 
            VersionPresentationKind presentationKind,
            VersionPresentationPart presentationParts) =>
            new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build()
                .Serialize(versionPresentation)
                .TrimEnd();
    }
}
