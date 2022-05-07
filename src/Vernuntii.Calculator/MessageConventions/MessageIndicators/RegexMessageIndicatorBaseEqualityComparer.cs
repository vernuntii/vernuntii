using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Compares two instances of type <see cref="RegexMessageIndicatorBase"/>.
    /// </summary>
    internal class RegexMessageIndicatorBaseEqualityComparer : EqualityComparer<RegexMessageIndicatorBase>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public new readonly static RegexMessageIndicatorBaseEqualityComparer Default = new RegexMessageIndicatorBaseEqualityComparer();

        /// <inheritdoc/>
        public override bool Equals(RegexMessageIndicatorBase? x, RegexMessageIndicatorBase? y) =>
            ReferenceEquals(x, y)
            || (MessageIndicatorBaseEqualityComparer.Default.Equals(x, y)
                && RegexMessageIndicatorEqualityComparer.Default.Equals(x, y));

        /// <inheritdoc/>
        public override int GetHashCode([DisallowNull] RegexMessageIndicatorBase obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(MessageIndicatorBaseEqualityComparer.Default.GetHashCode(obj));
            hashCode.Add(RegexMessageIndicatorEqualityComparer.Default.GetHashCode(obj));
            return hashCode.ToHashCode();
        }
    }
}
