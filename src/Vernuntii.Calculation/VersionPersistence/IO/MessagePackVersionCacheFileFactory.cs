using MessagePack;
using MessagePack.Formatters;
using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.VersionPersistence.IO;

internal class MessagePackVersionCacheFileFactory : IVersionCacheFileFactory
{
    public static MessagePackVersionCacheFileFactory Of(
        IEnumerable<IVersionCacheFormatter>? versionCacheFormatters,
        IEnumerable<IVersionCacheDeformatter> versionCacheDeformatters)
    {
        var immutableVersionCacheFormatters = versionCacheFormatters?.ToArray() ?? Array.Empty<IVersionCacheFormatter>();
        var immutableVersionCacheDeformatters = versionCacheDeformatters?.ToArray() ?? Array.Empty<IVersionCacheDeformatter>();
        var formatterResolver = new SingleFormatterResolver(new VersionCacheMessagePackFormatter(immutableVersionCacheFormatters, immutableVersionCacheDeformatters));
        var serializerOptions = new MessagePackSerializerOptions(formatterResolver);
        return new MessagePackVersionCacheFileFactory(serializerOptions);
    }

    /// <summary>
    /// The time to wait until lock attempt is aborted. Default are 10 seconds.
    /// </summary>
    private const int CacheLockAttemptSeconds = 10 * 1000;

    private readonly MessagePackSerializerOptions _serializerOptions;

    public MessagePackVersionCacheFileFactory(MessagePackSerializerOptions serializerOptions) =>
        _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));

    public IVersionCacheFile Open(string filePath) => new MessagePackVersionCacheFile(filePath, CacheLockAttemptSeconds, _serializerOptions);

    private class SingleFormatterResolver : IFormatterResolver
    {
        private readonly IMessagePackFormatter<IVersionCache> _formatter;

        public SingleFormatterResolver(IMessagePackFormatter<IVersionCache> formatter) =>
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));

        public IMessagePackFormatter<T> GetFormatter<T>() =>
            (IMessagePackFormatter<T>)(object)_formatter;
    }
}
