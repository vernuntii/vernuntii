using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Conventional commit message indicator.
    /// </summary>
    public class ConventionalCommitsMessageIndicator : RegexMessageIndicator
    {
        /// <summary>
        /// Default instance of type this type.
        /// </summary>
        public readonly static ConventionalCommitsMessageIndicator Default = new ConventionalCommitsMessageIndicator();

        /// <inheritdoc/>
        public override string IndicatorName { get; } = "ConventionalCommits";

        /// <inheritdoc/>
        protected override Regex MajorRegex { get; } = new Regex(@"^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(\\([\\w\\s-]*\\))?(!:|:.*\\n\\n((.+\\n)+\\n)?BREAKING CHANGE:\\s.+)");
        /// <inheritdoc/>
        protected override Regex MinorRegex { get; } = new Regex(@"^(feat)(\\([\\w\\s-]*\\))?:");
        /// <inheritdoc/>
        protected override Regex PatchRegex { get; } = new Regex(@"^(fix)(\\([\\w\\s-]*\\))?:");
    }
}
