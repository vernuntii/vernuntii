using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Compares two instances of type <see cref="IRegexMessageIndicator"/>.
    /// </summary>
    internal class RegexMessageIndicatorEqualityComparer : EqualityComparer<IRegexMessageIndicator>
    {
        /// <summary>
        /// Default instance of this type.
        /// </summary>
        public new readonly static RegexMessageIndicatorEqualityComparer Default = new RegexMessageIndicatorEqualityComparer();

        /// <inheritdoc/>
        public override bool Equals(IRegexMessageIndicator? x, IRegexMessageIndicator? y) =>
            ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.MajorRegex?.ToString() == y.MajorRegex?.ToString()
                && x.MinorRegex?.ToString() == y.MinorRegex?.ToString()
                && x.PatchRegex?.ToString() == y.PatchRegex?.ToString());

        /// <inheritdoc/>
        public override int GetHashCode([DisallowNull] IRegexMessageIndicator obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.MajorRegex?.ToString(), StringComparer.InvariantCulture);
            hashCode.Add(obj.MinorRegex?.ToString(), StringComparer.InvariantCulture);
            hashCode.Add(obj.PatchRegex?.ToString(), StringComparer.InvariantCulture);
            return hashCode.ToHashCode();
        }
    }
}
