using Teronis.IO.FileLocking;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// Represents the cache representive hash code of a git directory.
    /// </summary>
    internal class VersionHashFile
    {
        private const string RefsTagDirectory = "refs/tags";
        private const string RefsHeadsDirectory = "refs/heads";
        private const string PackedRefsDirectory = "packed-refs";
        private const string HashFileExtension = ".hash";

        private readonly VersionHashFileOptions _options;
        private readonly ICacheIdentifierProvider _cacheIdentifierProvider;
        private readonly IVersionCacheDirectory _cacheDirectory;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cacheIdentifierProvider"></param>
        /// <param name="cacheDirectory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VersionHashFile(
            VersionHashFileOptions options,
            ICacheIdentifierProvider cacheIdentifierProvider,
            IVersionCacheDirectory cacheDirectory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _cacheIdentifierProvider = cacheIdentifierProvider ?? throw new ArgumentNullException(nameof(cacheIdentifierProvider));
            _cacheDirectory = cacheDirectory ?? throw new ArgumentNullException(nameof(cacheDirectory));
        }

        /// <summary>
        /// Checks if recache is required. Precisely it calculates a hash (again) of configuration
        /// file and git-specific files and compares it to existing (if existing) hash file. If these
        /// hashes differ *or* the data inside the cache file (not to be confused with the hash file)
        /// got invalid (e.g. last access time exceeded) then a recache is required.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>
        /// The version cache when recache is not required. Is definitively <see langword="null"/> if
        /// returned boolean is <see langword="false"/>.
        /// </returns>
        public async Task<UpToDateResult<string>> IsHashUpToDateOtherwiseUpdate()
        {
            _cacheDirectory.CreateCacheDirectoryIfNotExisting();

            var configFile = _options.ConfigFile;
            var gitDirectory = _options.GitDirectory;
            var refsTagDirectory = Path.Combine(gitDirectory, RefsTagDirectory);
            var refsHeadDirectory = Path.Combine(gitDirectory, RefsHeadsDirectory);
            var packedRefsFile = Path.Combine(gitDirectory, PackedRefsDirectory);
            var hashFile = Path.Combine(_cacheDirectory.CacheDirectoryPath, $"{_cacheIdentifierProvider.CacheId}{HashFileExtension}");

            var filesHashCode = new FilesHashCode();

            if (configFile != null) {
                filesHashCode.AddFile(configFile);
            }

            if (Directory.Exists(refsTagDirectory)) {
                filesHashCode.AddDirectory(refsTagDirectory);
            }

            if (Directory.Exists(refsHeadDirectory)) {
                filesHashCode.AddDirectory(refsHeadDirectory);
            }

            if (File.Exists(packedRefsFile)) {
                filesHashCode.AddFile(packedRefsFile);
            }

            using var upToDateFileStream = FileStreamLocker.Default.WaitUntilAcquired(hashFile)
                ?? throw new InvalidOperationException($"Cannot acquire lock on up-to-date file: {hashFile}");

            using var upToDateFileBuffer = new MemoryStream((int)upToDateFileStream.Length);
            upToDateFileStream.CopyTo(upToDateFileBuffer);

            var upToDateHashCodeBytes = await filesHashCode.ToHashCodeAsync().ConfigureAwait(false);
            var upToDateFileHashCode = upToDateFileBuffer.ToArray();
            var isUpToDateHashEqual = upToDateFileHashCode.SequenceEqual(upToDateHashCodeBytes);

            if (!isUpToDateHashEqual) {
                string reason;

                if (upToDateFileStream.Length == 0) {
                    reason = "Hash is inexistent";
                } else {
                    upToDateFileStream.SetLength(0);
                    upToDateFileStream.Flush();
                    reason = "Hash has changed";
                }

                using var binaryWriter = new BinaryWriter(upToDateFileStream);
                binaryWriter.Write(upToDateHashCodeBytes);
                return UpToDateResult<string>.NotUpToDate(reason);
            }

            return UpToDateResult<string>.UpToDate;
        }
    }
}
