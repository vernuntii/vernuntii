namespace Vernuntii.HeightVersioning.Transforming
{
    internal class HeightPlaceholderParser : IHeightPlaceholderParser
    {
        public readonly static HeightPlaceholderParser Default = new HeightPlaceholderParser();

        public HeightPlaceholderType Parse(ReadOnlySpan<char> placeholder, out object? placeholderContent)
        {
            if (placeholder.Length == 0) {
                placeholderContent = null;
                return HeightPlaceholderType.Empty;
            }

            if (!placeholder.StartsWith("{") && !placeholder.StartsWith("}")) {
                throw new ArgumentException("Placeholder must be enclosed with '{' and '}");
            } else {
                placeholder = placeholder[1..^1];
            }

            if (placeholder.Length == 0) {
                placeholderContent = null;
                return HeightPlaceholderType.Identifiers;
            }

            if (placeholder.Equals("y", StringComparison.Ordinal)) {
                placeholderContent = null;
                return HeightPlaceholderType.Height;
            }

            if (int.TryParse(placeholder, out var identifierIndex)) {
                placeholderContent = identifierIndex;
                return HeightPlaceholderType.IdentifierIndex;
            }

            throw new ArgumentException("Placeholder is not well formatted");
        }
    }
}
