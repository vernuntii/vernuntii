using System.Runtime.Serialization;

namespace Vernuntii.VersionPersistence.Serialization;

[Serializable]
internal class VersionCacheSerializationException : Exception
{
    public VersionCacheSerializationException()
    {
    }

    public VersionCacheSerializationException(string? message) : base(message)
    {
    }

    public VersionCacheSerializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VersionCacheSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
