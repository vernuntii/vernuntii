using System.Text.Json.Serialization;

namespace Vernuntii.Text.RegularExpressions
{
    /// <summary>
    /// Represents an escape.
    /// </summary>
    internal sealed record RegexEscape : IRegexEscape
    {
        /// <inheritdoc />
        public string? Pattern { get; init; }

        public string? Replacement {
            get => _isReplacementAvailable
                ? _replacement
                : throw new InvalidOperationException($"{nameof(Replacement)} is not available but {nameof(GetReplacement)} is");

            init {
                _replacement = value;
                _isReplacementAvailable = true;
                GetReplacement = _ => _replacement ?? string.Empty;
            }
        }

        public Func<string, string>? GetReplacement { private set; get; }

        private string? _replacement;
        private bool _isReplacementAvailable = true;

        public RegexEscape() { }

        public RegexEscape(string? pattern) => Pattern = pattern;

        [JsonConstructor]
        public RegexEscape(string? pattern, string? replacement)
        {
            Pattern = pattern;
            Replacement = replacement;
        }

        public RegexEscape(string? pattern, Func<string, string>? getReplacement)
        {
            Pattern = pattern;
            SetReplacementGetter(getReplacement);
        }

        /// <summary>
        /// Creates an copy of <paramref name="regexEscape" />.
        /// </summary>
        /// <param name="regexEscape"></param>
        public RegexEscape(IRegexEscape regexEscape)
        {
            Pattern = regexEscape.Pattern;
            SetReplacementGetter(regexEscape.GetReplacement);
        }

        private void SetReplacementGetter(Func<string, string>? getReplacement)
        {
            _replacement = null;
            _isReplacementAvailable = false;
            GetReplacement = getReplacement;
        }
    }
}
