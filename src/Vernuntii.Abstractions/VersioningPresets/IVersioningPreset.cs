using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.VersionIncrementFlows;
using Vernuntii.VersionIncrementing;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A versioning preset.
    /// </summary>
    public interface IVersioningPreset : IEquatable<IVersioningPreset>
    {
        /// <summary>
        /// Describes whether the version does increment.
        /// </summary>
        VersionIncrementMode IncrementMode { get; }

        /// <summary>
        /// Describes how an increment of a version part may flow to another version.
        /// </summary>
        IVersionIncrementFlow IncrementFlow { get; }

        /// <summary>
        /// Message convention.
        /// </summary>
        IMessageConvention MessageConvention { get; }

        /// <summary>
        /// Height convention.
        /// </summary>
        IHeightConvention HeightConvention { get; }
    }
}
