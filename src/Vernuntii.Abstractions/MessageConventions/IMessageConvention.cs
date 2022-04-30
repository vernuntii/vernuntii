using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// Represents a message convention for major, minor and patch.
    /// </summary>
    public interface IMessageConvention : IEquatable<IMessageConvention>
    {
        /// <summary>
        /// Major indicators.
        /// </summary>
        IReadOnlyCollection<IMessageIndicator>? MajorIndicators { get; }
        /// <summary>
        /// Minor indicators.
        /// </summary>
        IReadOnlyCollection<IMessageIndicator>? MinorIndicators { get; }
        /// <summary>
        /// Patch indicators.
        /// </summary>
        IReadOnlyCollection<IMessageIndicator>? PatchIndicators { get; }
    }
}
