using System.Runtime.Serialization;

namespace Vernuntii.Coroutines;

/// <summary>
/// Thrown by cancellable suspending functions if the coroutine is cancelled while it is suspending. It indicates normal cancellation of a coroutine. 
/// </summary>
internal class CancellationException : Exception
{
    public CancellationException()
    {
    }

    public CancellationException(string? message) : base(message)
    {
    }

    public CancellationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CancellationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
