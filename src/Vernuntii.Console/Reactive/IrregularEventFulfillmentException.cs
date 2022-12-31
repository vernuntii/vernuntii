using System.Runtime.Serialization;

namespace Vernuntii.Reactive;

[Serializable]
internal class IrregularEventFulfillmentException : Exception
{
    public IrregularEventFulfillmentException()
    {
    }

    public IrregularEventFulfillmentException(string? message) : base(message)
    {
    }

    public IrregularEventFulfillmentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected IrregularEventFulfillmentException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
