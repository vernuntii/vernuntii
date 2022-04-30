namespace Vernuntii.MessageConventions.MessageIndicators
{
    public abstract class MessageIndicator : IMessageIndicator
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

        public bool IsMessageIndicating(string? message, VersionPart partToIndicate) => partToIndicate switch {
            _ when message == null => false,
            VersionPart.Major => IsMessageIndicatingMajor(message),
            VersionPart.Minor => IsMessageIndicatingMinor(message),
            VersionPart.Patch => IsMessageIndicatingPatch(message),
            _ => false
        };
    }
}
