using MessagePack;

namespace Vernuntii.VersionPersistence.Serialization;

public class GitVersionCacheFormatter : IVersionCacheFormatter
{
    public void Serialize(ref MessagePackWriter writer, IImmutableVersionCacheDataTuples dataTuples, MessagePackSerializerOptions options)
    {
        writer.Write(dataTuples.GetDataOrDefault(GitVersionCacheParts.BranchName));
        writer.Write(dataTuples.GetDataOrDefault(GitVersionCacheParts.BranchTip));
    }
}
