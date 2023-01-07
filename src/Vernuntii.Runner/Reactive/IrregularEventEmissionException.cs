using System.Runtime.Serialization;

namespace Vernuntii.Reactive;

[Serializable]
internal class IrregularEventEmissionException : Exception
{
    public IrregularEventEmissionException()
    {
    }

    public IrregularEventEmissionException(string? message) : base(message)
    {
    }

    public IrregularEventEmissionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected IrregularEventEmissionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
