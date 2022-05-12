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
        uint InitialHeight { get; }

        /// <summary>
        /// <see langword="true"/> hides the initial height.
        /// </summary>
        bool HideInitialHeight { get; }
    }
}
