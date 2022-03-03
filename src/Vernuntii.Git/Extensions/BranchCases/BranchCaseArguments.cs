using Vernuntii.Text.RegularExpressions;

namespace Vernuntii.Extensions.BranchCases
{
    /// <inheritdoc />
    internal sealed record BranchCaseArguments : IBranchCaseArguments
    {
        internal const string ConfigurationExtensionName = "Configuration";

        private static List<RegexEscape>? ConvertEscapes(IEnumerable<IRegexEscape>? escapes) => escapes == null
            ? null
            : new List<RegexEscape>(escapes.Select(escape => new RegexEscape(escape)));

        /// <inheritdoc />
        public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>();

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

        IReadOnlyCollection<IRegexEscape>? IBranchCaseArguments.PreReleaseEscapes => PreReleaseEscapes;

        /// <inheritdoc />
        public string? SearchPreRelease { get; init; }

        /// <inheritdoc />
        public List<RegexEscape>? SearchPreReleaseEscapes { get; init; }

        IReadOnlyCollection<IRegexEscape>? IBranchCaseArguments.SearchPreReleaseEscapes => SearchPreReleaseEscapes;

        /// <summary>
        /// Creates an instance of <see cref="BranchCaseArguments" />.
        /// </summary>
        public BranchCaseArguments()
        {
        }

        /// <summary>
        /// Creates an copy of <paramref name="arguments" />.
        /// </summary>
        /// <param name="arguments"></param>
        public BranchCaseArguments(IBranchCaseArguments arguments)
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
