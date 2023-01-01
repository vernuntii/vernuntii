using System.Runtime.Serialization;

namespace Vernuntii.VersionPersistence.Presentation;

[Serializable]
internal class VersionPresentationException : Exception
{
    public VersionPresentationException()
    {
    }

    public VersionPresentationException(string? message) : base(message)
    {
    }

    public VersionPresentationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VersionPresentationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
