using YamlDotNet.Serialization;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class SemanticVersionPresentationYamlSerializer : ISemanticVersionPresentationSerializer
    {
        public readonly static SemanticVersionPresentationYamlSerializer Default = new SemanticVersionPresentationYamlSerializer();

        public string? SerializeSemanticVersion(
            object versionPresentation, 
            SemanticVersionPresentationKind presentationKind,
            SemanticVersionPresentationPart presentationParts) =>
            new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build()
                .Serialize(versionPresentation)
                .TrimEnd();
    }
}
