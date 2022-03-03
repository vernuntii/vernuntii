using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vernuntii.SemVer.Json
{
    /// <summary>
    /// Serializer and deserializer for <see cref="SemanticVersion"/>.
    /// </summary>
    public class SemanticVersionJsonConverter : JsonConverter<SemanticVersion>
    {
        /// <inheritdoc/>
        public override SemanticVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var versionString = reader.GetString();

            if (versionString == null) {
                return null;
            }

            return SemanticVersion.Parse(versionString);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString());
    }
}
