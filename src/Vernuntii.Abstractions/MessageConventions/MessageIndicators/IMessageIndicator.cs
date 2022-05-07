namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Message indicator.
    /// </summary>
    public interface IMessageIndicator : IEquatable<IMessageIndicator>
    {
        /// <summary>
        /// Indicates a message for a version part.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="partToIndicate"></param>
        /// <returns>True if message indicates <paramref name="partToIndicate"/>.</returns>
        bool IsMessageIndicating(string? message, VersionPart partToIndicate);
    }
}
