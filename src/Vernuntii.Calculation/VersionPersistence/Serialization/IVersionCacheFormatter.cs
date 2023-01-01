using MessagePack;

namespace Vernuntii.VersionPersistence.Serialization;

public interface IVersionCacheFormatter
{
    void Serialize(ref MessagePackWriter writer, IImmutableVersionCacheDataTuples versionCache, MessagePackSerializerOptions options);
}
