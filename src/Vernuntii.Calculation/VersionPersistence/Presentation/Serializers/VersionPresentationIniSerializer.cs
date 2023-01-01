using System.Globalization;
using System.Text;
using Teronis.Text;

namespace Vernuntii.VersionPersistence.Presentation.Serializers
{
    internal class VersionPresentationIniSerializer : IVersionPresentationSerializer
    {
        public static readonly VersionPresentationIniSerializer Default = new();

        public string? SerializeSemanticVersion(
            IVersionCache versionCache,
            VersionPresentationKind presentationKind,
            VersionPresentationParts presentationParts)
        {
            var stringBuilder = new StringBuilder();
            var stringSeparator = new StringSeparator(Environment.NewLine);

            if (presentationKind == VersionPresentationKind.Value) {
                var singlePart = presentationParts.AssertSingleDueToValueKind();
                _ = versionCache.TryGetData(singlePart, out var value, out _);
                stringBuilder.Append(CultureInfo.InvariantCulture, $"{value}");
            } else {
                foreach (var presentationPart in presentationParts) {
                    VersionPresentationSerializerHelpers.GetData(versionCache, presentationPart, out var data, out _);
                    stringBuilder.Append(CultureInfo.InvariantCulture, $"{presentationPart.Name}={data}");
                    stringSeparator.SetSeparator(stringBuilder);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
