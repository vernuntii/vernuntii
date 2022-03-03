using Vernuntii.MessageVersioning.HeightConventions.Ruling;

namespace Vernuntii.MessageVersioning.HeightConventions
{
    internal record class HeightConvention : IHeightConvention
    {
        public HeightPosition Position { get; }
        public HeightRuleDictionary? Rules { get; init; }

        public HeightConvention(HeightPosition position) =>
            Position = position;
    }
}
