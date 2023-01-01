using MessagePack;

namespace Vernuntii.VersionPersistence.Serialization;

public class GitVersionCacheDeformatter : IVersionCacheDeformatter
{
    public void Deserialize(ref MessagePackReader reader, VersionCacheDataTuples dataTuples, MessagePackSerializerOptions options)
    {
        dataTuples.AddData(GitVersionCacheParts.BranchName, reader.ReadString());
        dataTuples.AddData(GitVersionCacheParts.BranchTip, reader.ReadString());
    }
}
