using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Message indicator using Regular Expressions.
    /// </summary>
    public abstract record RegexMessageIndicatorBase : MessageIndicatorBase, IRegexMessageIndicator
    {
        /// <inheritdoc/>
        public abstract Regex? MajorRegex { get; init; }
        /// <inheritdoc/>
        public abstract Regex? MinorRegex { get; init; }
        /// <inheritdoc/>
        public abstract Regex? PatchRegex { get; init; }

        /// <summary>
        /// Builder for creating a copy of this instance.
        /// </summary>
        public RegexMessageIndicatorBuilder With =>
            new RegexMessageIndicatorBuilder(this);

        /// <inheritdoc/>
        protected override bool IsMessageIndicatingMajor(string message) =>
            MajorRegex is not null && MajorRegex.IsMatch(message);

        /// <inheritdoc/>
        protected override bool IsMessageIndicatingMinor(string message) =>
            MinorRegex is not null && MinorRegex.IsMatch(message);

        /// <inheritdoc/>
        protected override bool IsMessageIndicatingPatch(string message) =>
            PatchRegex is not null && PatchRegex.IsMatch(message);

        bool IEquatable<IRegexMessageIndicator>.Equals(IRegexMessageIndicator? other) =>
            other is RegexMessageIndicatorBase otherIndicator
            ? RegexMessageIndicatorBaseEqualityComparer.Default.Equals(this, otherIndicator)
            : RegexMessageIndicatorEqualityComparer.Default.Equals(this, other);

        bool IEquatable<IMessageIndicator>.Equals(IMessageIndicator? other) =>
            ((IRegexMessageIndicator)this).Equals(other as IRegexMessageIndicator);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            RegexMessageIndicatorBaseEqualityComparer.Default.GetHashCode(this);
    }
}
