using System.Runtime.Serialization;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Versioning preset was missing.
    /// </summary>
    public class VersioningPresetMissingException : Exception
    {
        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public VersioningPresetMissingException()
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public VersioningPresetMissingException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        public VersioningPresetMissingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        protected VersioningPresetMissingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
