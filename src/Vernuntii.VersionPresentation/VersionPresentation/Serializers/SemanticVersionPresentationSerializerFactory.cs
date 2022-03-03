namespace Vernuntii.VersionPresentation.Serializers
{
    internal static class SemanticVersionPresentationSerializerFactory
    {
        public static ISemanticVersionPresentationSerializer CreateSerializer(SemanticVersionPresentationView presentationView) => presentationView switch {
            SemanticVersionPresentationView.Text => SemanticVersionPresentationIniSerializer.Default,
            SemanticVersionPresentationView.Json => SemanticVersionPresentationJsonSerializer.Default,
            SemanticVersionPresentationView.Yaml => SemanticVersionPresentationYamlSerializer.Default,
            _ => throw new NotSupportedException()
        };
    }
}
