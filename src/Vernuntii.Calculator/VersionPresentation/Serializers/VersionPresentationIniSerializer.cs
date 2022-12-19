using System.Globalization;
using System.Reflection;
using System.Text;

namespace Vernuntii.VersionPresentation.Serializers
{
    internal class VersionPresentationIniSerializer : IVersionPresentationSerializer
    {
        public static readonly VersionPresentationIniSerializer Default = new();

        public string? SerializeSemanticVersion(
            object versionPresentation,
            VersionPresentationKind presentationKind,
            VersionPresentationPart presentationParts)
        {
            var properties = versionPresentation.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead);

            var stringBuilder = new StringBuilder();
            var stringSeparator = new StringSeparator(Environment.NewLine);

            if (presentationKind == VersionPresentationKind.Value) {
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{versionPresentation}");
            } else {
                foreach (var property in properties) {
                    var value = property.GetValue(versionPresentation);

                    if (value == null) {
                        continue;
                    }

                    stringBuilder.Append(CultureInfo.InvariantCulture, $"{property.Name}={value}");
                    stringSeparator.SetSeparator(stringBuilder);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
