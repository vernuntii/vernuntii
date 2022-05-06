using Vernuntii.HeightConventions.Rules;

namespace Vernuntii.HeightConventions
{
    /// <summary>
    /// Height convention.
    /// </summary>
    public interface IHeightConvention : IEquatable<IHeightConvention>
    {
        /// <summary>
        /// Position of height.
        /// </summary>
        HeightIdentifierPosition Position { get; }
        /// <summary>
        /// Height rules.
        /// </summary>
        IHeightRuleDictionary? Rules { get; }
        /// <summary>
        /// The start height when no height could be estimated.
        /// </summary>
        uint StartHeight { get; }
    }
}
