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
        /// <see langword="true"/> means when actual version has zero major and
        /// next version is indicating major, then instead minor is indicated.
        /// The same applies for minor. Patch indicates patch and won't shift.
        /// </summary>
        bool RightShiftWhenZeroMajor { get; }

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
