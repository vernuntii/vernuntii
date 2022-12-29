using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MessagePack;
using MessagePack.Formatters;
using Teronis.IO.FileLocking;
using Vernuntii.Text.Json;

namespace Vernuntii.VersionCaching.MessagePack;

internal class MessagePackVersionCacheFile : IDisposable, IManagedValueWriter<IVersionCache>
{
    public static readonly FileStreamLocker FileStreamLocker = new(new LockFileSystem());
    public static MessagePackSerializerOptions SerializerOptions = new(DefaultFormatterResolver.Instance);

    private readonly FileStream _stream;

    public MessagePackVersionCacheFile(string jsonFilePath, int lockTimeout)
    {
        _stream = FileStreamLocker.WaitUntilAcquired(jsonFilePath, lockTimeout)
            ?? throw new TimeoutException("Locking the cache file has been aborted due to timeout");
    }

    /// <inheritdoc/>
    public void Overwrite(IVersionCache value)
    {
        if (_stream.Length != 0) {
            _stream.SetLength(0);
            _stream.Flush();
        }

        MessagePackSerializer.Serialize(_stream, value, SerializerOptions);
    }

    public IVersionCache ReadValue(FileStream stream)
    {
        stream.Position = 0;

        return MessagePackSerializer.Deserialize<IVersionCache>(stream, SerializerOptions)
            ?? throw new JsonException($"A non-null serialized type of {typeof(DefaultVersionCache).FullName} was expected");
    }

    public bool TryReadValue([NotNullWhen(true)] out IVersionCache? value)
    {
        if (_stream.Length == 0) {
            value = null;
            return false;
        }

        value = ReadValue(_stream);
        return true;
    }

    public void Dispose() => _stream.Dispose();

    private class DefaultFormatterResolver : IFormatterResolver
    {
        public static readonly DefaultFormatterResolver Instance = new DefaultFormatterResolver();

        public IMessagePackFormatter<T> GetFormatter<T>() =>
            (IMessagePackFormatter<T>)(object)VersionCacheFormatter.Instance;
    }
}
