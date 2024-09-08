using System.Runtime.Serialization;

namespace Vernuntii.Infrastructure;

internal class Exception1 : Exception
{
    public Exception1()
    {
    }

    public Exception1(string? message) : base(message)
    {
    }

    public Exception1(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected Exception1(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
