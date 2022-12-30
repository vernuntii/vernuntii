using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.MessageConventions
{
    /// <summary>
    /// Extension methods for <see cref="IMessageConvention"/>.
    /// </summary>
    public static class MessageConventionExtensions
    {
        internal static bool IsMessageIndicating(IEnumerable<IMessageIndicator>? messageIndicators, string? message, VersionPart partToIndicate)
        {
            var enumerator = messageIndicators?.GetEnumerator();

            if (enumerator is not null && enumerator.MoveNext()) {
                do {
                    if (enumerator.Current.IsMessageIndicating(message, partToIndicate)) {
                        return true;
                    }
                } while (enumerator.MoveNext());
            }

            return false;
        }

        /// <summary>
        /// Indicates whether message is major.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <param name="message"></param>
        public static bool IsMessageIndicatingMajor(this IMessageConvention? messageConvention, string? message) =>
            IsMessageIndicating(messageConvention?.MajorIndicators, message, VersionPart.Major);

        /// <summary>
        /// Indicates whether message is minor.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <param name="message"></param>
        public static bool IsMessageIndicatingMinor(this IMessageConvention? messageConvention, string? message) =>
            IsMessageIndicating(messageConvention?.MinorIndicators, message, VersionPart.Minor);

        /// <summary>
        /// Indicates whether message is patch.
        /// </summary>
        /// <param name="messageConvention"></param>
        /// <param name="message"></param>
        public static bool IsMessageIndicatingPatch(this IMessageConvention? messageConvention, string? message) =>
            IsMessageIndicating(messageConvention?.PatchIndicators, message, VersionPart.Patch);
    }
}
