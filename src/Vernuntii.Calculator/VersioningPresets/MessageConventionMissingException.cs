using System.Runtime.Serialization;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Versioning preset was missing.
    /// </summary>
    public class MessageConventionMissingException : Exception
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageConventionMissingException()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageConventionMissingException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public MessageConventionMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected MessageConventionMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
