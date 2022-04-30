using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// A message convention.
    /// </summary>
    public record MessageConvention : IMessageConvention
    {
        private static bool Equals(IEnumerable<IMessageIndicator>? x, IEnumerable<IMessageIndicator>? y) =>
            ReferenceEquals(x, y)
            || x is not null
                && y is not null
                && x.SequenceEqual(y, MessageIndicatorNameComparer.Default);

        private static int GetHashCode(IEnumerable<IMessageIndicator>? enumerable)
        {
            if (enumerable is null) {
                return 0;
            }

            var hashCode = new HashCode();

            foreach (var indicator in enumerable) {
                hashCode.Add(MessageIndicatorNameComparer.Default.GetHashCode(indicator));
            }

            return hashCode.ToHashCode();
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMessageIndicator>? MajorIndicators { get; init; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMessageIndicator>? MinorIndicators { get; init; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMessageIndicator>? PatchIndicators { get; init; }

        /// <inheritdoc/>
        public bool Equals(IMessageConvention? other) =>
            other is not null
            && Equals(MajorIndicators, other.MajorIndicators)
            && Equals(MinorIndicators, other.MinorIndicators)
            && Equals(PatchIndicators, other.PatchIndicators);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(GetHashCode(MajorIndicators));
            hashCode.Add(GetHashCode(MinorIndicators));
            hashCode.Add(GetHashCode(PatchIndicators));
            return hashCode.ToHashCode();
        }
    }
}
