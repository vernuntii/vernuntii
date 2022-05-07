using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Conventional commit message indicator.
    /// </summary>
    public record ConventionalCommitsMessageIndicator : RegexMessageIndicatorBase
    {
        /// <summary>
        /// Default instance of type this type.
        /// </summary>
        public readonly static ConventionalCommitsMessageIndicator Default = new ConventionalCommitsMessageIndicator();

        /// <inheritdoc/>
        public override string IndicatorName { get; } = nameof(InbuiltMessageIndicator.ConventionalCommits);

        /// <inheritdoc/>
        public override Regex? MajorRegex { get; init; } = new Regex(@"^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(\\([\\w\\s-]*\\))?(!:|:.*\\n\\n((.+\\n)+\\n)?BREAKING CHANGE:\\s.+)");
        /// <inheritdoc/>
        public override Regex? MinorRegex { get; init; } = new Regex(@"^(feat)(\\([\\w\\s-]*\\))?:");
        /// <inheritdoc/>
        public override Regex? PatchRegex { get; init; } = new Regex(@"^(fix)(\\([\\w\\s-]*\\))?:");
    }
}
