using YamlDotNet.Serialization;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class VersionPresentationYamlSerializer : IVersionPresentationSerializer
    {
        public static readonly VersionPresentationYamlSerializer Default = new();

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
