using Vernuntii.MessageConventions;
using Vernuntii.MessageVersioning;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Extension methods for <see cref="IMessageConventionRegistry"/>.
    /// </summary>
    public static class MessageConventionRegistryExtensions
    {
        /// <summary>
        /// Gets message convention.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="messageConventionName"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IMessageConvention? GetMessageConvention(this IMessageConventionRegistry registry, string? messageConventionName) =>
            registry.MessageConventions[messageConventionName ?? nameof(InbuiltVersioningPreset.Default)];

        /// <summary>
        /// Gets message convention.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="InbuiltVersioningPreset"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IMessageConvention? GetMessageConvention(this IMessageConventionRegistry registry, InbuiltVersioningPreset? InbuiltVersioningPreset) =>
            registry.GetMessageConvention(InbuiltVersioningPreset?.ToString());

        /// <summary>
        /// Gets message convention.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="InbuiltMessageConvention"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IMessageConvention? GetMessageConvention(this IMessageConventionRegistry registry, InbuiltMessageConvention? InbuiltMessageConvention) =>
            registry.GetMessageConvention(InbuiltMessageConvention?.ToString());
    }
}
