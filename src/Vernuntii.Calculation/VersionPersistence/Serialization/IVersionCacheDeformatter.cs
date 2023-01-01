using MessagePack;

namespace Vernuntii.VersionPersistence.Serialization;

public interface IVersionCacheDeformatter
{
    void Deserialize(ref MessagePackReader reader, VersionCacheDataTuples dataTuples, MessagePackSerializerOptions options);
}
