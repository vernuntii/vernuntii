using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class VersionPresentationJsonSerializer : IVersionPresentationSerializer
    {
        public static readonly VersionPresentationJsonSerializer Default = new();

        public string? SerializeSemanticVersion(
            object versionPresentation,
            VersionPresentationKind presentationKind,
            VersionPresentationPart presentationParts) =>
            JsonSerializer.Serialize(
                versionPresentation,
                new JsonSerializerOptions() {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                });
    }
}
