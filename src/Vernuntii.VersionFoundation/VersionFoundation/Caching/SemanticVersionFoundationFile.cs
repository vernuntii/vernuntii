using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using Vernuntii.SemVer.Json;
using Teronis.IO.FileLocking;

namespace Vernuntii.VersionFoundation.Caching
{
    internal sealed class SemanticVersionFoundationFile<T> : IDisposable, ISemanticVersionFoundationWriter<T>
        where T : class
    {
        public readonly static FileStreamLocker FileStreamLocker = new FileStreamLocker(new LockFileSystem());

        private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        static SemanticVersionFoundationFile() => JsonSerializerOptions.Converters.Add(new SemanticVersionJsonConverter());

        public static T ReadPresentationFoundation(FileStream stream)
        {
            stream.Position = 0;

            return JsonSerializer.Deserialize<T>(stream, JsonSerializerOptions)
                ?? throw new JsonException($"A non-null serialized type of {typeof(T).FullName} was expected");
        }

        private readonly FileStream _stream;

        public SemanticVersionFoundationFile(string jsonFilePath, int lockTimeout)
        {
            _stream = FileStreamLocker.WaitUntilAcquired(jsonFilePath, lockTimeout)
                ?? throw new TimeoutException("Locking the cache file has been aborted due to timeout");
        }

        /// <inheritdoc/>
        public void WriteVersionFoundation(T value)
        {
            if (_stream.Length != 0) {
                _stream.SetLength(0);
            }

            JsonSerializer.Serialize(_stream, value, JsonSerializerOptions);
        }

        public bool TryReadPresentationFoundation([NotNullWhen(true)] out T? value)
        {
            if (_stream.Length == 0) {
                value = null;
                return false;
            }

            value = ReadPresentationFoundation(_stream);
            return true;
        }

        public void Dispose() => _stream.Dispose();
    }
}
