using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// A message convention.
    /// </summary>
    public record MessageConvention : IMessageConvention
    {
        /// <summary>
        /// An empty instance that does not have a single message indicator.
        /// </summary>
        public static readonly MessageConvention None = new();

        private static bool SequenceEqual(IEnumerable<IMessageIndicator>? x, IEnumerable<IMessageIndicator>? y) =>
            ReferenceEquals(x, y)
            || x is not null
                && y is not null
                && x.SequenceEqual(y);

        private static int GetHashCode(IEnumerable<IMessageIndicator>? enumerable)
        {
            if (enumerable is null) {
                return 0;
            }

            var hashCode = new HashCode();

            foreach (var indicator in enumerable) {
                hashCode.Add(indicator);
            }

            return hashCode.ToHashCode();
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMessageIndicator>? MajorIndicators { get; init; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMessageIndicator>? MinorIndicators { get; init; }

        /// <inheritdoc/>
        public IReadOnlyCollection<IMessageIndicator>? PatchIndicators { get; init; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageConvention()
        {
        }

        /// <summary>
        /// Creates a shallow copy of <paramref name="messageConvention"/>.
        /// </summary>
        /// <param name="messageConvention"></param>
        public MessageConvention(IMessageConvention messageConvention)
        {
            MajorIndicators = messageConvention.MajorIndicators;
            MinorIndicators = messageConvention.MinorIndicators;
            PatchIndicators = messageConvention.PatchIndicators;
        }

        /// <inheritdoc/>
        public virtual bool Equals(MessageConvention? other) =>
            ((IMessageConvention)this).Equals(other);

        /// <inheritdoc/>
        bool IEquatable<IMessageConvention>.Equals(IMessageConvention? other) =>
            other is not null
            && SequenceEqual(MajorIndicators, other.MajorIndicators)
            && SequenceEqual(MinorIndicators, other.MinorIndicators)
            && SequenceEqual(PatchIndicators, other.PatchIndicators);

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
