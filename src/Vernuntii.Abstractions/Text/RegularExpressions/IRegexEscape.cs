namespace Vernuntii.Text.RegularExpressions
{
    /// <summary>
    /// Represents an escape.
    /// </summary>
    public interface IRegexEscape
    {
        /// <summary>
        /// The pattern to find occurrences being escaped.
        /// </summary>
        string? Pattern { get; }
        /// <summary>
        /// The replacement to use to replace occurrences.
        /// </summary>
        Func<string, string>? GetReplacement { get; }
    }
}
