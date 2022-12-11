namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// A parser to parse a dotted identifier of SemVer-compatible version.
    /// </summary>
    public interface IDottedIdentifierParser
    {
        /// <summary>
        /// Tries to parse pre-release.
        /// </summary>
        /// <param name="versionPart"></param>
        /// <param name="dottedIdentifier"></param>
        /// <returns>Valid parse result if successful.</returns>
        IOptionalIdentifierParseResult<IEnumerable<string>> TryParseDottedIdentifier(SemanticVersionPart versionPart, string? dottedIdentifier);
    }
}
