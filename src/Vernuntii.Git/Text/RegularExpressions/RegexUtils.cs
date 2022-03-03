using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Vernuntii.Text.RegularExpressions
{
    /// <summary>
    /// Utility functions for Regular Expressions.
    /// </summary>
    internal static class RegexUtils
    {
        /// <summary>
        /// Checks if pattern starts and ends with '/'.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        public static bool IsRegexPattern(string? value, [NotNullWhen(true)] out string? pattern)
        {
            if (value != null && value.Length > 2
                && value.StartsWith('/') && value.EndsWith('/')) {
                pattern = value[1..^1];
                return true;
            }

            pattern = null;
            return false;
        }

        /// <summary>
        /// Replaces all occurrences of pattern.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <param name="getReplacement"></param>
        /// <returns>Value with replacements.</returns>
        public static string Replace(string value, string pattern, Func<string, string> getReplacement)
        {
            if (IsRegexPattern(pattern, out var regexPattern)) {
                return Regex.Replace(value, regexPattern, match => getReplacement(match.Value));
            } else {
                return value.Replace(pattern, getReplacement(value), StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// Checks if value matches the pattern.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pattern"></param>
        /// <returns>True if matched.</returns>
        public static bool IsMatch(string? value, string? pattern)
        {
            if (IsRegexPattern(pattern, out var regexPattern)) {
                return Regex.IsMatch(value ?? string.Empty, regexPattern);
            } else {
                return StringComparer.Ordinal.Equals(value, pattern);
            }
        }

        /// <summary>
        /// Escapes <paramref name="value"/> by rules of <paramref name="regexEscapes"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="regexEscapes"></param>
        /// <returns>Escaped value.</returns>
        [return: NotNullIfNotNull("name")]
        public static string? Escape(string? value, IReadOnlyCollection<IRegexEscape>? regexEscapes)
        {
            if (string.IsNullOrEmpty(value)
                || regexEscapes is null || regexEscapes.Count == 0) {
                return value;
            }

            return regexEscapes.Aggregate(
                value,
                static (a, b) => {
                    if (string.IsNullOrEmpty(b.Pattern)) {
                        return a;
                    }

                    return Replace(a, b.Pattern, b.GetReplacement ?? (static _ => string.Empty));
                });
        }
    }
}
