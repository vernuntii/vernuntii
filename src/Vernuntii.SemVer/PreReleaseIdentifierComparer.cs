using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.SemVer;

/// <summary>
/// A comparer for the "pre-release"-part of a semantic version.
/// </summary>
public static class PreReleaseIdentifierComparer
{
    /// <summary>
    /// The default instance of <see cref="PreReleaseIdentifierComparer"/>.
    /// </summary>
    public static readonly IDotSplittedIdentifierComparer Default = new ComparerImpl();

    private class ComparerImpl : AbstractDotSplittedIndentifierComparer
    {
        protected override StringComparison StringComparisonType { get; } = StringComparison.OrdinalIgnoreCase;
        protected override SemanticVersionPart IdentifierParsingVersionPart { get; } = SemanticVersionPart.PreRelease;
        protected override IDottedIdentifierParser IdentifierParser { get; } = PreReleaseIdentifierParser.Strict;

        /// <summary>
        /// Pre-release identifiers are compared as numbers if they are numeric, otherwise they will be compared
        /// as strings.
        /// </summary>
        private int CompareeIdentifier(string preRelease, string otherPreRelease)
        {
            int result;

            // Check for numeric versions
            var isPreReleaseNumeric = int.TryParse(preRelease, out var versionNumber);
            var isOtherPreReleaseNumeric = int.TryParse(otherPreRelease, out var otherVersionNumber);

            // If both versions are numeric we can compare as numbers
            if (isPreReleaseNumeric && isOtherPreReleaseNumeric) {
                result = versionNumber.CompareTo(otherVersionNumber);
            } else if (isPreReleaseNumeric || isOtherPreReleaseNumeric) {
                // Numeric pre-release identifiers come before string pre-release identifiers
                if (isPreReleaseNumeric) {
                    result = -1;
                } else {
                    result = 1;
                }
            } else {
                // We can ignore 2.0.0 case sensitive comparison, instead compare insensitively as specified in 2.1.0.
                // See https://github.com/semver/semver/pull/276 for more.
                result = StringComparer.Compare(preRelease, otherPreRelease);
            }

            return result;
        }

        /// <summary>
        /// Compares pre-release identifiers.
        /// </summary>
        protected override int CompareIdentifiers(IEnumerable<string> identifiers, IEnumerable<string> otherIdentifiers)
        {
            var result = 0;

            var identifierEnumerator = identifiers.GetEnumerator();
            var otherIdentifierEnumerator = otherIdentifiers.GetEnumerator();

            var hasNextIdentifier = identifierEnumerator.MoveNext();
            var hasNextOtherIdentifier = otherIdentifierEnumerator.MoveNext();

            while (hasNextIdentifier || hasNextOtherIdentifier) {
                if (!hasNextIdentifier && hasNextOtherIdentifier) {
                    return -1;
                }

                if (hasNextIdentifier && !hasNextOtherIdentifier) {
                    return 1;
                }

                result = CompareeIdentifier(identifierEnumerator.Current, otherIdentifierEnumerator.Current);

                if (result != 0) {
                    return result;
                }

                hasNextIdentifier = identifierEnumerator.MoveNext();
                hasNextOtherIdentifier = otherIdentifierEnumerator.MoveNext();
            }

            return result;
        }
    }
}
