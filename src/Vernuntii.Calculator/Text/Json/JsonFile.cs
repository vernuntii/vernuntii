using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using Vernuntii.SemVer.Json;
using Teronis.IO.FileLocking;
using Vernuntii.SemVer.Json.System;

namespace Vernuntii.Text.Json
{
    internal sealed class JsonFile<T> : IDisposable, IValueWriter<T>
        where T : class
    {
        public readonly static FileStreamLocker FileStreamLocker = new FileStreamLocker(new LockFileSystem());

        private static JsonSerializerOptions SerializerOptions = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        static JsonFile() => SerializerOptions.Converters.Add(VersionStringJsonConverter.Default);

        public static T ReadValue(FileStream stream)
        {
            stream.Position = 0;

            return JsonSerializer.Deserialize<T>(stream, SerializerOptions)
                ?? throw new JsonException($"A non-null serialized type of {typeof(T).FullName} was expected");
        }

        private readonly FileStream _stream;

        public JsonFile(string jsonFilePath, int lockTimeout)
        {
            _stream = FileStreamLocker.WaitUntilAcquired(jsonFilePath, lockTimeout)
                ?? throw new TimeoutException("Locking the cache file has been aborted due to timeout");
        }

        /// <inheritdoc/>
        public void WriteValue(T value)
        {
            if (_stream.Length != 0) {
                _stream.SetLength(0);
                _stream.Flush();
            }

            JsonSerializer.Serialize(_stream, value, SerializerOptions);
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
