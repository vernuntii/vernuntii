using MessagePack;
using MessagePack.Formatters;
using Vernuntii.SemVer;

namespace Vernuntii.VersionCaching.MessagePack;

internal class SemanticVersionFormatter : IMessagePackFormatter<ISemanticVersion>
{
    public static readonly SemanticVersionFormatter Instance = new SemanticVersionFormatter();

    public void Serialize(ref MessagePackWriter writer, ISemanticVersion value, MessagePackSerializerOptions options) =>
        writer.Write(value.ToString());

    public ISemanticVersion Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
        SemanticVersion.Parse(reader.ReadString());
}
