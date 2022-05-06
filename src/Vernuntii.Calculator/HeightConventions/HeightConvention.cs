using Vernuntii.Collections;
using Vernuntii.Extensions;
using Vernuntii.HeightConventions.Rules;

namespace Vernuntii.HeightConventions
{
    internal sealed record class HeightConvention : IHeightConvention, IEquatable<HeightConvention>
    {
        public readonly static HeightConvention None = new HeightConvention(HeightIdentifierPosition.None);

        public HeightIdentifierPosition Position { get; }
        public IHeightRuleDictionary? Rules { get; init; }
        public uint StartHeight { get; init; }

        public HeightConvention(HeightIdentifierPosition position) =>
            Position = position;

        public bool Equals(IHeightConvention? other) =>
            other is not null
            && Position == other.Position
            && (ReferenceEquals(Rules, other.Rules)
                || (Rules is not null
                    && other.Rules is not null
                    && Rules.SequenceEqual(other.Rules, KeyValuePairEqualityComparer<int, IHeightRule>.Default)))
            && StartHeight == other.StartHeight;

        public bool Equals(HeightConvention? other) =>
            Equals((IHeightConvention?)other);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Position);

            if (Rules is not null) {
                hashCode.AddEnumerable(Rules, KeyValuePairEqualityComparer<int, IHeightRule>.Default);
            }

            hashCode.Add(StartHeight);
            return hashCode.ToHashCode();
        }
    }
}
