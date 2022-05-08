using System.Runtime.Serialization;

namespace Vernuntii.Configuration
{
    public class ConfigurationValidationException : Exception
    {
        public ConfigurationValidationException()
        {
        }

        public ConfigurationValidationException(string message) : base(message)
        {
        }

        public ConfigurationValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
