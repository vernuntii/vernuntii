namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Message indicator.
    /// </summary>
    public abstract record MessageIndicatorBase : IMessageIndicator, IEquatable<MessageIndicatorBase>
    {
        /// <inheritdoc/>
        public abstract string IndicatorName { get; }

        /// <summary>
        /// Checks whether message indicates next major version.
        /// </summary>
        /// <param name="message"></param>
        protected abstract bool IsMessageIndicatingMajor(string message);

        /// <summary>
        /// Checks whether message indicates next minor version.
        /// </summary>
        /// <param name="message"></param>
        protected abstract bool IsMessageIndicatingMinor(string message);

        /// <summary>
        /// Checks whether message indicates next patch version.
        /// </summary>
        /// <param name="message"></param>
        protected abstract bool IsMessageIndicatingPatch(string message);

        /// <inheritdoc/>
        public bool IsMessageIndicating(string? message, VersionPart partToIndicate) => partToIndicate switch {
            _ when message == null => false,
            VersionPart.Major => IsMessageIndicatingMajor(message),
            VersionPart.Minor => IsMessageIndicatingMinor(message),
            VersionPart.Patch => IsMessageIndicatingPatch(message),
            _ => false
        };

        /// <inheritdoc/>
        public virtual bool Equals(MessageIndicatorBase? other) =>
            ((IMessageIndicator)this).Equals(other);

        bool IEquatable<IMessageIndicator>.Equals(IMessageIndicator? other) =>
            other is MessageIndicatorBase otherIndiactor
            ? MessageIndicatorBaseEqualityComparer.Default.Equals(this, otherIndiactor)
            : false;

        /// <inheritdoc/>
        public override int GetHashCode() =>
            MessageIndicatorBaseEqualityComparer.Default.GetHashCode(this);
    }
}
