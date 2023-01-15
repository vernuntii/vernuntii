using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vernuntii.VersionPersistence.Presentation.Serializers
{
    internal class VersionPresentationJsonSerializer : IVersionPresentationSerializer
    {
        public static readonly VersionPresentationJsonSerializer Default = new();

        public string SerializeSemanticVersion(
            IVersionCache versionCache,
            VersionPresentationKind presentationKind,
            VersionPresentationParts presentationParts) =>
            JsonSerializer.Serialize(
                versionCache,
                new JsonSerializerOptions() {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    Converters = {
                        new VersionCacheJsonConverter(presentationKind, presentationParts)
                    },

                });

        private class VersionCacheJsonConverter : JsonConverter<IVersionCache>
        {
            private readonly VersionPresentationKind _presentationKind;
            private readonly VersionPresentationParts _presentationParts;

            public VersionCacheJsonConverter(VersionPresentationKind presentationKind, VersionPresentationParts presentationParts)
            {
                _presentationKind = presentationKind;
                _presentationParts = presentationParts;
            }

            public override IVersionCache? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                throw new NotImplementedException();

            public override void Write(Utf8JsonWriter writer, IVersionCache versionCache, JsonSerializerOptions options)
            {
                if (_presentationKind == VersionPresentationKind.Value) {
                    var singlePart = _presentationParts.AssertSingleDueToValueKind();

                    if (versionCache.TryGetData(singlePart, out var value, out var valueType)) {
                        JsonSerializer.Serialize(writer, value, valueType, options);
                    }

                    writer.WriteNullValue();
                } else {
                    writer.WriteStartObject();

                    foreach (var presentationPart in _presentationParts) {
                        VersionPresentationSerializerHelpers.GetData(versionCache, presentationPart, out var data, out var dataType);
                        writer.WritePropertyName(presentationPart.Name);
                        JsonSerializer.Serialize(writer, data, dataType, options);
                    }

                    writer.WriteEndObject();
                }
            }
        }
    }
}
