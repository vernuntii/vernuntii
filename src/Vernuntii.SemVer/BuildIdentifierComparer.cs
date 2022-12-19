using Vernuntii.SemVer.Parser;
using Vernuntii.SemVer.Parser.Parsers;

namespace Vernuntii.SemVer;

/// <summary>
/// A comparer for the "build"-part of a semantic version.
/// </summary>
public static class BuildIdentifierComparer
{
    /// <summary>
    /// The default instance of <see cref="PreReleaseIdentifierComparer"/>.
    /// </summary>
    public static readonly IDotSplittedIdentifierComparer Default = new ComparerImpl();

    private class ComparerImpl : AbstractDotSplittedIndentifierComparer
    {
        protected override StringComparison StringComparisonType { get; } = StringComparison.Ordinal;
        protected override SemanticVersionPart IdentifierParsingVersionPart { get; } = SemanticVersionPart.Build;
        protected override IDottedIdentifierParser IdentifierParser { get; } = PreReleaseIdentifierParser.Strict;

        /// <summary>
        /// Compares build identifiers.
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

                result = StringComparer.Compare(identifierEnumerator.Current, otherIdentifierEnumerator.Current);

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
