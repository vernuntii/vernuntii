using Newtonsoft.Json;

namespace Vernuntii.SemVer.Json.Newtonsoft
{
    /// <summary>
    /// Serializer and deserializer for <see cref="SemanticVersion"/>.
    /// </summary>
    public class VersionStringJsonConverter : JsonConverter<SemanticVersion>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public readonly static VersionStringJsonConverter Default = new VersionStringJsonConverter();

        /// <inheritdoc/>
        public override SemanticVersion? ReadJson(JsonReader reader, Type objectType, SemanticVersion? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var versionValue = reader.Value;

            if (versionValue == null) {
                return null;
            } else if (versionValue is not string versionString) {
                throw new JsonException("Value was expected to be a string");
            } else {
                return SemanticVersion.Parse(versionString);
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, SemanticVersion? value, JsonSerializer serializer) =>
            writer.WriteValue(value?.ToString());
    }
}
