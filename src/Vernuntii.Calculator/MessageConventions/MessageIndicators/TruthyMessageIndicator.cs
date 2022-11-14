namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Message indicator that always indicates.
    /// </summary>
    public sealed class TruthyMessageIndicator : IMessageIndicator
    {
        /// <summary>
        /// Default instance of type this type.
        /// </summary>
        public static readonly TruthyMessageIndicator Default = new();

        /// <inheritdoc/>
        public bool IsMessageIndicating(string? message, VersionPart partToIndicate) => true;

        bool IEquatable<IMessageIndicator>.Equals(IMessageIndicator? other) =>
            other is TruthyMessageIndicator;
    }
}
