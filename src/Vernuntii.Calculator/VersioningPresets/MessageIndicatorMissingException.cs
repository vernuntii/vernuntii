using System.Runtime.Serialization;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Versioning preset was missing.
    /// </summary>
    public class MessageIndicatorMissingException : Exception
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageIndicatorMissingException()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageIndicatorMissingException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageIndicatorMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected MessageIndicatorMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
