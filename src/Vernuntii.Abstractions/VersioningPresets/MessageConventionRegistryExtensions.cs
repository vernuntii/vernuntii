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
            registry.MessageConventions[messageConventionName ?? nameof(VersioningPresetKind.Default)];

        /// <summary>
        /// Gets message convention.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="versioningPresetKind"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IMessageConvention? GetMessageConvention(this IMessageConventionRegistry registry, VersioningPresetKind? versioningPresetKind) =>
            registry.GetMessageConvention(versioningPresetKind?.ToString());

        /// <summary>
        /// Gets message convention.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="messageConventionKind"></param>
        /// <exception cref="ArgumentException"></exception>
        public static IMessageConvention? GetMessageConvention(this IMessageConventionRegistry registry, MessageConventionKind? messageConventionKind) =>
            registry.GetMessageConvention(messageConventionKind?.ToString());
    }
}
