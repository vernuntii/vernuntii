namespace Vernuntii.VersionTransformers
{
    internal class HeightPlaceholderParser : IHeightPlaceholderParser
    {
        public readonly static HeightPlaceholderParser Default = new HeightPlaceholderParser();

        public object? ParsePlaceholder(ReadOnlySpan<char> placeholder, out HeightPlaceholderType placeholderType)
        {
            if (placeholder.Length == 0) {
                placeholderType = HeightPlaceholderType.Empty;
                return null;
            }

            if (!placeholder.StartsWith("%")) {
                throw new ArgumentException("Placeholder does not start with %");
            } else {
                placeholder = placeholder[1..];
            }

            if (placeholder.Length == 0) {
                placeholderType = HeightPlaceholderType.Identifiers;
                return null;
            }

            if (placeholder.Equals("y", StringComparison.Ordinal)) {
                placeholderType = HeightPlaceholderType.Height;
                return null;
            }

            if (int.TryParse(placeholder, out var identifierIndex)) {
                placeholderType = HeightPlaceholderType.IdentifierIndex;
                return identifierIndex;
            }

            throw new ArgumentException("Placeholder is not well formatted");
        }
    }
}
