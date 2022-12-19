using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vernuntii.Text.Json
{
    internal sealed class JsonFile<T> : IDisposable, IManagedValueWriter<T>
        where T : class
    {
        public static readonly FileStreamLocker FileStreamLocker = new(new LockFileSystem());

        public JsonSerializerContext SerializerContext { get; }

        private readonly FileStream _stream;

        public JsonFile(string jsonFilePath, int lockTimeout, JsonSerializerContext serializerContext)
        {
            _stream = FileStreamLocker.WaitUntilAcquired(jsonFilePath, lockTimeout)
                ?? throw new TimeoutException("Locking the cache file has been aborted due to timeout");

            SerializerContext = serializerContext ?? throw new ArgumentNullException(nameof(serializerContext));
        }

        /// <inheritdoc/>
        public void Overwrite(T value)
        {
            if (_stream.Length != 0) {
                _stream.SetLength(0);
                _stream.Flush();
            }

            using var writer = new Utf8JsonWriter(_stream);
            JsonSerializer.Serialize(writer, value, typeof(T), SerializerContext);
        }

        public T ReadValue(FileStream stream)
        {
            stream.Position = 0;

            return JsonSerializer.Deserialize<T>(stream, SerializerContext.Options)
                ?? throw new JsonException($"A non-null serialized type of {typeof(T).FullName} was expected");
        }

        public bool TryReadValue([NotNullWhen(true)] out T? value)
        {
            if (_stream.Length == 0) {
                value = null;
                return false;
            }

            value = ReadValue(_stream);
            return true;
        }

        public void Dispose() => _stream.Dispose();
    }
}
