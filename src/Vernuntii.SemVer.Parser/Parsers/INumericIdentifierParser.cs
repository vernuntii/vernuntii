namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// A parser to parse one of the numeric identifier of a SemVer-compatible version.
    /// </summary>
    public interface INumericIdentifierParser
    {
        /// <summary>
        /// Tries to parse a numeric identifier.
        /// </summary>
        /// <param name="numericIdentifier"></param>
        /// <returns>Valid parse result if successful.</returns>
        INotNullableIdentifierParseResult<uint?> TryParseNumericIdentifier(string? numericIdentifier);
    }
}
