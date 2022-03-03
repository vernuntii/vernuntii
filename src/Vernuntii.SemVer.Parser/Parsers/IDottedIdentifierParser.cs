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
        /// <param name="dottedIdentifier"></param>
        /// <returns>Valid parse result if successful.</returns>
        IdentifierParseResult<IEnumerable<string>> TryParseDottedIdentifier(string? dottedIdentifier);
    }
}
