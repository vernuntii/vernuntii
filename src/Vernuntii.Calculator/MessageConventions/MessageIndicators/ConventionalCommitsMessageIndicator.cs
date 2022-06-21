using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Conventional commit message indicator.
    /// </summary>
    public record ConventionalCommitsMessageIndicator : RegexMessageIndicatorBase
    {
        private static Regex SinglelineRegex(string pattern) =>
            new Regex(pattern, RegexOptions.Singleline);

        /// <summary>
        /// Default instance of type this type.
        /// </summary>
        public readonly static ConventionalCommitsMessageIndicator Default = new ConventionalCommitsMessageIndicator();

        /// <inheritdoc/>
        public override string IndicatorName { get; } = nameof(InbuiltMessageIndicator.ConventionalCommits);

        /// <inheritdoc/>
        public override Regex? MajorRegex { get; init; } = SinglelineRegex(/* language=regex */"""^(feat)(\([\w\s-]*\))?(!:|:.*\n\n((.+\n)+\n)?BREAKING CHANGE:\s.+)""");
        /// <inheritdoc/>
        public override Regex? MinorRegex { get; init; } = SinglelineRegex(/* language=regex */"""^(feat)(\([\w\s-]*\))?:""");
        /// <inheritdoc/>
        public override Regex? PatchRegex { get; init; } = SinglelineRegex(/* language=regex */"""^(fix)(\([\w\s-]*\))?:""");
    }
}
