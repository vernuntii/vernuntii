using System.Runtime.Serialization;

namespace Vernuntii.VersionPersistence.Presentation.Serializers;

[Serializable]
internal class VersionPresentationKindException : Exception
{
    public VersionPresentationKindException()
    {
    }

    public VersionPresentationKindException(string? message) : base(message)
    {
    }

    public VersionPresentationKindException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VersionPresentationKindException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
