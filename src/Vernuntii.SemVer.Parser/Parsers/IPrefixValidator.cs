namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// Validator for prefix.
    /// </summary>
    public interface IPrefixValidator
    {
        /// <summary>
        /// Validates prefix.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns>True if prefix is valid.</returns>
        bool ValidatePrefix(string? prefix);
    }
}
