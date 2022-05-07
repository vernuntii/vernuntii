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
        public readonly static TruthyMessageIndicator Default = new TruthyMessageIndicator();

        /// <inheritdoc/>
        public bool IsMessageIndicating(string? message, VersionPart partToIndicate) => true;

        bool IEquatable<IMessageIndicator>.Equals(IMessageIndicator? other) =>
            other is TruthyMessageIndicator;
    }
}
