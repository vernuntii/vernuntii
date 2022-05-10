using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    /// <inheritdoc />
    internal sealed record BranchCase : IBranchCase
    {
        internal const string ConfigurationExtensionName = "Configuration";

        private static List<RegexEscape>? ConvertEscapes(IEnumerable<IRegexEscape>? escapes) => escapes == null
            ? null
            : new List<RegexEscape>(escapes.Select(escape => new RegexEscape(escape)));

        /// <inheritdoc />
        public IDictionary<string, object?> Extensions { get; } = new Dictionary<string, object?>();

        /// <inheritdoc />
        public string? IfBranch { get; init; }

        /// <inheritdoc />
        public string? Branch { get; init; }

        /// <inheritdoc />
        public string? SinceCommit { get; init; }

        /// <inheritdoc />
        public string? PreRelease { get; init; }

        /// <inheritdoc />
        public List<RegexEscape>? PreReleaseEscapes { get; init; }

        IReadOnlyCollection<IRegexEscape>? IBranchCase.PreReleaseEscapes => PreReleaseEscapes;

        /// <inheritdoc />
        public string? SearchPreRelease { get; init; }

        /// <inheritdoc />
        public List<RegexEscape>? SearchPreReleaseEscapes { get; init; }

        IReadOnlyCollection<IRegexEscape>? IBranchCase.SearchPreReleaseEscapes => SearchPreReleaseEscapes;

        /// <summary>
        /// Creates an instance of <see cref="BranchCase" />.
        /// </summary>
        public BranchCase()
        {
        }

        /// <summary>
        /// Creates an copy of <paramref name="arguments" />.
        /// </summary>
        /// <param name="arguments"></param>
        public BranchCase(IBranchCase arguments)
        {
            Extensions = arguments.Extensions;
            IfBranch = arguments.IfBranch;
            Branch = arguments.Branch;
            SinceCommit = arguments.SinceCommit;
            PreRelease = arguments.PreRelease;
            PreReleaseEscapes = ConvertEscapes(arguments.PreReleaseEscapes);
            SearchPreRelease = arguments.SearchPreRelease;
            SearchPreReleaseEscapes = ConvertEscapes(arguments.SearchPreReleaseEscapes);
        }
    }
}
