using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A versioning preset.
    /// </summary>
    public interface IVersioningPreset
    {
        /// <summary>
        /// Increment mode.
        /// </summary>
        VersionIncrementMode IncrementMode { get; }

        /// <summary>
        /// Message convention.
        /// </summary>
        IMessageConvention? MessageConvention { get; }

        /// <summary>
        /// Height convention.
        /// </summary>
        IHeightConvention? HeightConvention { get; }
    }
}
