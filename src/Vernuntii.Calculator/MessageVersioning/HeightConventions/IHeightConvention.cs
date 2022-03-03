using Vernuntii.MessageVersioning.HeightConventions.Ruling;

namespace Vernuntii.MessageVersioning.HeightConventions
{
    /// <summary>
    /// Represents a height convention.
    /// </summary>
    public interface IHeightConvention
    {
        /// <summary>
        /// Position of height.
        /// </summary>
        HeightPosition Position { get; }
        /// <summary>
        /// Height rules.
        /// </summary>
        HeightRuleDictionary? Rules { get; }
    }
}
