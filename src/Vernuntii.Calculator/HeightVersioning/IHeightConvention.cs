using Vernuntii.HeightVersioning.Ruling;

namespace Vernuntii.HeightVersioning
{
    /// <summary>
    /// Height convention.
    /// </summary>
    public interface IHeightConvention
    {
        /// <summary>
        /// Position of height.
        /// </summary>
        HeightIdentifierPosition Position { get; }
        /// <summary>
        /// Height rules.
        /// </summary>
        HeightRuleDictionary? Rules { get; }
        /// <summary>
        /// The start height when no height could be estimated.
        /// </summary>
        uint StartHeight { get; }
    }
}
