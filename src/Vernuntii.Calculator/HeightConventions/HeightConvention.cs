using Vernuntii.HeightConventions.Rules;

namespace Vernuntii.HeightConventions
{
    internal record class HeightConvention : IHeightConvention
    {
        public readonly static HeightConvention None = new HeightConvention(HeightIdentifierPosition.None);

        public HeightIdentifierPosition Position { get; }
        public IHeightRuleDictionary? Rules { get; init; }
        public uint StartHeight { get; init; }

        public HeightConvention(HeightIdentifierPosition position) =>
            Position = position;
    }
}
