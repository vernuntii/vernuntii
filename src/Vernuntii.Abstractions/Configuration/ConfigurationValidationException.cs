using System.Runtime.Serialization;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// An exception indicating that the configuration validation failed.
    /// </summary>
    public class ConfigurationValidationException : Exception
    {
        /// <inheritdoc/>
        public ConfigurationValidationException()
        {
        }

        /// <inheritdoc/>
        public ConfigurationValidationException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public ConfigurationValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected ConfigurationValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
