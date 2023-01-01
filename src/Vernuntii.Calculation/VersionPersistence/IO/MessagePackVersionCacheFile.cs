using MessagePack;
using Teronis.IO.FileLocking;
using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.VersionPersistence.IO;

internal class MessagePackVersionCacheFile : IVersionCacheFile
{
    private static readonly FileStreamLocker s_fileStreamLocker = new(new LockFileSystem());

    public bool CanRead => _stream.Length != 0;

    private MessagePackSerializerOptions _serializerOptions;
    private readonly FileStream _stream;

    public MessagePackVersionCacheFile(string filePath, int fileLockAttemptTime, MessagePackSerializerOptions serializerOptions)
    {
        _stream = s_fileStreamLocker.WaitUntilAcquired(filePath, fileLockAttemptTime)
            ?? throw new TimeoutException("Locking the cache file has been aborted due to timeout");

        _serializerOptions = serializerOptions;
    }

    /// <inheritdoc/>
    public void Overwrite(IVersionCache value)
    {
        if (_stream.Length != 0) {
            _stream.SetLength(0);
            _stream.Flush();
        }

        try {
            MessagePackSerializer.Serialize(_stream, value, _serializerOptions);
        } catch (Exception error) {
            throw new VersionCacheSerializationException("An exception occured during the serialization of the version cache: " + error.Message, error);
        }
    }

    public IVersionCache ReadCache()
    {
        _stream.Position = 0;

        try {
            return MessagePackSerializer.Deserialize<IVersionCache>(_stream, _serializerOptions)
                ?? throw new VersionCacheSerializationException($"A non-null serialized type of {typeof(VersionCache).FullName} was expected");
        } catch (Exception error) {
            throw new VersionCacheSerializationException("An exception occured during the deserialization of the version cache: " + error.Message, error);
        }
    }

    public void Dispose() => 
        _stream.Dispose();
}
