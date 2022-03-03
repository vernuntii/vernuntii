namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// A parser to parse one of the numeric identifier of a SemVer-compatible version.
    /// </summary>
    public interface IVersionNumberParser
    {
        /// <summary>
        /// Tries to parse a numeric identifier.
        /// </summary>
        /// <param name="numericIdentifier"></param>
        /// <returns>Valid parse result if successful.</returns>
        IdentifierParseResult<uint?> TryParseVersionNumber(string? versionNumber);
    }
}
