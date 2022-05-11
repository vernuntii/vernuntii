using Vernuntii.Collections;
using Vernuntii.Extensions;
using Vernuntii.HeightConventions.Rules;

namespace Vernuntii.HeightConventions
{
    /// <summary>
    /// The height convention describes where a height can be placed.
    /// </summary>
    public sealed record class HeightConvention : IHeightConvention, IEquatable<HeightConvention>
    {
        /// <summary>
        /// Height convention where height is nowhere.
        /// </summary>
        public readonly static HeightConvention None = new HeightConvention(HeightIdentifierPosition.None);

        /// <summary>
        /// Height convention with height in pre-release after first dot.
        /// </summary>
        public readonly static HeightConvention InPreReleaseAfterFirstDot = new HeightConvention(HeightIdentifierPosition.PreRelease) {
            Rules = HeightRuleDictionary.AfterFirstDotRules
        };

        /// <inheritdoc/>
        public HeightIdentifierPosition Position { get; }
        /// <inheritdoc/>
        public IHeightRuleDictionary? Rules { get; init; }
        /// <inheritdoc/>
        public uint StartHeight { get; init; }

        /// <summary>
        /// Creates instance of this type.
        /// </summary>
        /// <param name="position"></param>
        public HeightConvention(HeightIdentifierPosition position) =>
            Position = position;

        /// <inheritdoc/>
        public bool Equals(IHeightConvention? other) =>
            other is not null
            && Position == other.Position
            && (ReferenceEquals(Rules, other.Rules)
                || (Rules is not null
                    && other.Rules is not null
                    && Rules.SequenceEqual(other.Rules, KeyValuePairEqualityComparer<int, IHeightRule>.Default)))
            && StartHeight == other.StartHeight;

        /// <inheritdoc/>
        public bool Equals(HeightConvention? other) =>
            Equals((IHeightConvention?)other);

        /// <inheritdoc/>
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
