using System;
using System.Runtime.Serialization;

namespace Vernuntii.Plugins;

internal class NextVersionApiException : InvalidOperationException
{
    public NextVersionApiException()
    {
    }

    public NextVersionApiException(string message) : base(message)
    {
    }

    public NextVersionApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected NextVersionApiException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
