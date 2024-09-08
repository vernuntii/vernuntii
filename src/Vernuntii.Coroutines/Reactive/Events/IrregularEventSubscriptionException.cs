using System.Runtime.Serialization;

namespace Vernuntii.Reactive.Events;

[Serializable]
internal class IrregularEventSubscriptionException : Exception
{
    public IrregularEventSubscriptionException()
    {
    }

    public IrregularEventSubscriptionException(string? message) : base(message)
    {
    }

    public IrregularEventSubscriptionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected IrregularEventSubscriptionException(SerializationInfo info, StreamingContext emissionBacklog) : base(info, emissionBacklog)
    {
    }
}
