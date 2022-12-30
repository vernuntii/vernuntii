namespace Vernuntii.VersionPresentation.Serializers
{
    internal static class VersionPresentationSerializerFactory
    {
        public static IVersionPresentationSerializer CreateSerializer(VersionPresentationView presentationView) => presentationView switch {
            VersionPresentationView.Text => VersionPresentationIniSerializer.Default,
            VersionPresentationView.Json => VersionPresentationJsonSerializer.Default,
            VersionPresentationView.Yaml => VersionPresentationYamlSerializer.Default,
            _ => throw new NotSupportedException()
        };
    }
}
