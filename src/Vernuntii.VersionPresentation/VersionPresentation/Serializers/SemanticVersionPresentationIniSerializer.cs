using System.Globalization;
using System.Reflection;
using System.Text;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class SemanticVersionPresentationIniSerializer : ISemanticVersionPresentationSerializer
    {
        public readonly static SemanticVersionPresentationIniSerializer Default = new SemanticVersionPresentationIniSerializer();

        public string? SerializeSemanticVersion(
            object versionPresentation,
            SemanticVersionPresentationKind presentationKind,
            SemanticVersionPresentationPart presentationParts)
        {
            var properties = versionPresentation.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead);

            var stringBuilder = new StringBuilder();

            if (presentationKind == SemanticVersionPresentationKind.Value) {
                stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{versionPresentation}");
            } else {
                foreach (var property in properties) {
                    var value = property.GetValue(versionPresentation);

                    if (value == null) {
                        continue;
                    }

                    stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{property.Name}={value}");
                }
            }

            return stringBuilder.ToString();
        }
    }
}
