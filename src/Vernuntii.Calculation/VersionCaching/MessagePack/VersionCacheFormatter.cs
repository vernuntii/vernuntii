using MessagePack;
using MessagePack.Formatters;

namespace Vernuntii.VersionCaching.MessagePack;

internal class VersionCacheFormatter : IMessagePackFormatter<IVersionCache>
{
    public static readonly VersionCacheFormatter Instance = new VersionCacheFormatter();

    public void Serialize(ref MessagePackWriter writer, IVersionCache value, MessagePackSerializerOptions options)
    {
        SemanticVersionFormatter.Instance.Serialize(ref writer, value.Version, options);
        writer.Write(value.BranchName);
        writer.Write(value.BranchTip);
        NullableDateTimeFormatter.Instance.Serialize(ref writer, value.ExpirationTime, options);
        NullableDateTimeFormatter.Instance.Serialize(ref writer, value.LastAccessTime, options);
    }

    public IVersionCache Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var version = SemanticVersionFormatter.Instance.Deserialize(ref reader, options);
        var branchName = reader.ReadString();
        var branchTip = reader.ReadString();
        var expirationTime = NullableDateTimeFormatter.Instance.Deserialize(ref reader, options);
        var lastAccessTime = NullableDateTimeFormatter.Instance.Deserialize(ref reader, options);

        return new DefaultVersionCache(version, branchName, branchTip, expirationTime) {
            LastAccessTime = lastAccessTime
        };
    }
}
