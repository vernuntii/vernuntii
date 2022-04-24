using Vernuntii.HeightVersioning.Ruling;

namespace Vernuntii.HeightVersioning
{
    internal record class HeightConvention : IHeightConvention
    {
        public HeightIdentifierPosition Position { get; }
        public HeightRuleDictionary? Rules { get; init; }
        public uint StartHeight { get; init; }

        public HeightConvention(HeightIdentifierPosition position) =>
            Position = position;
    }
}
