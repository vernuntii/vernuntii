using MessagePack;
using MessagePack.Formatters;

namespace Vernuntii.VersionPersistence.Serialization;

internal class VersionCacheMessagePackFormatter : IMessagePackFormatter<IVersionCache>
{
    public static readonly VersionCacheMessagePackFormatter Instance = new VersionCacheMessagePackFormatter(formatters: null, deformatters: null);

    private IEnumerable<IVersionCacheFormatter>? _formatters;
    private IEnumerable<IVersionCacheDeformatter>? _deformatters;

    public VersionCacheMessagePackFormatter(IEnumerable<IVersionCacheFormatter>? formatters, IEnumerable<IVersionCacheDeformatter>? deformatters)
    {
        _formatters = formatters;
        _deformatters = deformatters;
    }

    private void SerializeDataTuples(ref MessagePackWriter writer, IImmutableVersionCacheDataTuples dataTuples, MessagePackSerializerOptions options)
    {
        if (_formatters is not null) {
            foreach (var formatter in _formatters) {
                formatter.Serialize(ref writer, dataTuples, options);
            }
        }
    }

    public void Serialize(ref MessagePackWriter writer, IVersionCache value, MessagePackSerializerOptions options)
    {
        SemanticVersionMessagePackFormatter.Instance.Serialize(ref writer, value.Version, options);
        NullableDateTimeFormatter.Instance.Serialize(ref writer, value.ExpirationTime.Time, options);
        NullableDateTimeFormatter.Instance.Serialize(ref writer, value.LastAccessTime, options);
        SerializeDataTuples(ref writer, value.GetAdditionalDataTuples(), options);
    }

    private IImmutableVersionCacheDataTuples DeserializeDataTuples(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var dataTuples = new VersionCacheDataTuples();

        if (_deformatters is not null) {
            foreach (var deformatter in _deformatters) {
                deformatter.Deserialize(ref reader, dataTuples, options);
            }
        }

        return new ImmutableVersionCacheDataTuples(dataTuples);
    }

    public IVersionCache Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var version = SemanticVersionMessagePackFormatter.Instance.Deserialize(ref reader, options);
        var expirationTime = NullableDateTimeFormatter.Instance.Deserialize(ref reader, options).ToExpirationTime();
        var lastAccessTime = NullableDateTimeFormatter.Instance.Deserialize(ref reader, options);
        var dataTuples = DeserializeDataTuples(ref reader, options);

        return new VersionCache(version, dataTuples, skipDataLookup: true) {
            ExpirationTime = expirationTime,
            LastAccessTime = lastAccessTime
        };
    }
}
