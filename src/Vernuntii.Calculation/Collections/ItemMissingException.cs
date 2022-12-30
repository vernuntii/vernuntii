using System.Runtime.Serialization;

namespace Vernuntii.Collections
{
    /// <summary>
    /// Item was missing.
    /// </summary>
    public class ItemMissingException : Exception
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public ItemMissingException()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public ItemMissingException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public ItemMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected ItemMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
