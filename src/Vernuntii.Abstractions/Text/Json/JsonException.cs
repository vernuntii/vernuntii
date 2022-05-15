using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Vernuntii.Text.Json
{
    /// <inheritdoc/>
    public class JsonException : Exception
    {
        /// <inheritdoc/>
        public JsonException()
        {
        }

        /// <inheritdoc/>
        public JsonException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public JsonException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected JsonException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
