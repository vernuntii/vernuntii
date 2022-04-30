using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Message indicator using Regular Expressions.
    /// </summary>
    public abstract class RegexMessageIndicator : MessageIndicator
    {
        /// <inheritdoc/>
        protected abstract Regex MajorRegex { get; }
        /// <inheritdoc/>
        protected abstract Regex MinorRegex { get; }
        /// <inheritdoc/>
        protected abstract Regex PatchRegex { get; }

        /// <inheritdoc/>
        protected override bool IsMessageIndicatingMajor(string message) => MajorRegex.IsMatch(message);

        /// <inheritdoc/>
        protected override bool IsMessageIndicatingMinor(string message) => MinorRegex.IsMatch(message);

        /// <inheritdoc/>
        protected override bool IsMessageIndicatingPatch(string message) => PatchRegex.IsMatch(message);
    }
}
