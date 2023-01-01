using MessagePack;
using MessagePack.Formatters;

namespace Vernuntii.VersionPersistence.Serialization;

internal class DecoupledMessagePackFormatter : IMessagePackFormatter<IVersionCache>
{
    private readonly IMessagePackFormatter<IVersionCache> _formatter;
    private readonly IMessagePackFormatter<IVersionCache> _deformatter;

    public DecoupledMessagePackFormatter(IMessagePackFormatter<IVersionCache> formatter, IMessagePackFormatter<IVersionCache> deformatter)
    {
        _formatter = formatter;
        _deformatter = deformatter;
    }

    public void Serialize(ref MessagePackWriter writer, IVersionCache value, MessagePackSerializerOptions options) =>
        _formatter.Serialize(ref writer, value, options);

    public IVersionCache Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
        _deformatter.Deserialize(ref reader, options);
}
