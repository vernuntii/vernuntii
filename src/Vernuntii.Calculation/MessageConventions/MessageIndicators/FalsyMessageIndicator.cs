namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Message indicator that does not indicate.
    /// </summary>
    public sealed class FalsyMessageIndicator : IMessageIndicator
    {
        /// <summary>
        /// Default instance of type this type.
        /// </summary>
        public static readonly FalsyMessageIndicator Default = new();

        /// <inheritdoc/>
        public bool IsMessageIndicating(string? message, VersionPart partToIndicate) => false;

        bool IEquatable<IMessageIndicator>.Equals(IMessageIndicator? other) =>
            other is FalsyMessageIndicator;
    }
}
