using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using Vernuntii.SemVer.Json;
using Teronis.IO.FileLocking;
using Newtonsoft.Json;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;
using NewtonsoftJsonSerializer = Newtonsoft.Json.JsonSerializer;
using VernuntiiJsonException = Vernuntii.Text.Json.JsonException;

namespace Vernuntii.VersionFoundation.Caching
{
    internal sealed class VersionFoundationFile<T> : IDisposable, IVersionFoundationWriter<T>
        where T : class
    {
        public readonly static FileStreamLocker FileStreamLocker = new FileStreamLocker(new LockFileSystem());

        private static JsonSerializerOptions SystemJsonSerializerOptions = new JsonSerializerOptions() {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static NewtonsoftJsonSerializer NewtonsoftJsonSerializer = new NewtonsoftJsonSerializer();

        static VersionFoundationFile()
        {
            SystemJsonSerializerOptions.Converters.Add(SemVer.Json.System.VersionStringJsonConverter.Default);
            NewtonsoftJsonSerializer.Converters.Add(SemVer.Json.Newtonsoft.VersionStringJsonConverter.Default);
        }

        public static T ReadPresentationFoundation(FileStream stream)
        {
            stream.Position = 0;

            using var streamReader = new StreamReader(stream, leaveOpen: true);
            using var jsonReader = new JsonTextReader(streamReader);

            return NewtonsoftJsonSerializer.Deserialize<T>(jsonReader)
                ?? throw new VernuntiiJsonException($"A non-null serialized type of {typeof(T).FullName} was expected");
        }

        private readonly FileStream _stream;

        public VersionFoundationFile(string jsonFilePath, int lockTimeout)
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

            SystemJsonSerializer.Serialize(_stream, value, SystemJsonSerializerOptions);
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
