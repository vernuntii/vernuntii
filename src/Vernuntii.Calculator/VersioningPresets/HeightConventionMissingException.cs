using System.Runtime.Serialization;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Versioning preset was missing.
    /// </summary>
    public class HeightConventionMissingException : Exception
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public HeightConventionMissingException()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public HeightConventionMissingException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public HeightConventionMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected HeightConventionMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
