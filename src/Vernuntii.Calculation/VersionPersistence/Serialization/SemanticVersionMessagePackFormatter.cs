using MessagePack;
using MessagePack.Formatters;
using Vernuntii.SemVer;

namespace Vernuntii.VersionPersistence.Serialization;

internal class SemanticVersionMessagePackFormatter : IMessagePackFormatter<ISemanticVersion>
{
    public static readonly SemanticVersionMessagePackFormatter Instance = new SemanticVersionMessagePackFormatter();

    public void Serialize(ref MessagePackWriter writer, ISemanticVersion value, MessagePackSerializerOptions options) =>
        writer.Write(value.ToString());

    public ISemanticVersion Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
        SemanticVersion.Parse(reader.ReadString());
}
