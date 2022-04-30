using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class SemanticVersionPresentationJsonSerializer : ISemanticVersionPresentationSerializer
    {
        public readonly static SemanticVersionPresentationJsonSerializer Default = new SemanticVersionPresentationJsonSerializer();

        public string? SerializeSemanticVersion(
            object versionPresentation, 
            SemanticVersionPresentationKind presentationKind,
            SemanticVersionPresentationPart presentationParts) => 
            JsonSerializer.Serialize(
                versionPresentation,
                new JsonSerializerOptions() {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                });
    }
}
