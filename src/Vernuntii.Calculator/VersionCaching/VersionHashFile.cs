using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Vernuntii.Cryptography;

namespace Vernuntii.VersionCaching
{
    /// <summary>
    /// Represents the cache representive hash code of a git directory.
    /// </summary>
    public class VersionHashFile
    {
        private const string _refsTagDirectory = "refs/tags";
        private const string _refsHeadsDirectory = "refs/heads";
        private const string _packedRefsDirectory = "packed-refs";
        private const string _hashExtension = ".hash";

        private readonly VersionHashFileOptions _options;
        private readonly IVersionCacheManager _cacheManager;
        private readonly IVersionCacheDirectory _cacheDirectory;
        private readonly ILogger _logger;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="cacheManager"></param>
        /// <param name="cacheDirectory"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VersionHashFile(
            VersionHashFileOptions options,
            IVersionCacheManager cacheManager,
            IVersionCacheDirectory cacheDirectory,
            ILogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
            _cacheDirectory = cacheDirectory ?? throw new ArgumentNullException(nameof(cacheDirectory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks if recache is required. Precisely it calculates a hash (again) of configuration
        /// file and git-specific files and compares it to existing (if existing) hash file. If these
        /// hashes differ *or* the data inside the cache file (not to be confused with the hash file)
        /// got invalid (e.g. last access time exceeded) then a recache is required.
        /// </summary>
        /// <param name="versionCache">
        /// The version cache when recache is not required. Is definitively <see langword="null"/> if
        /// returned boolean is <see langword="false"/>.
        /// </param>
        /// <exception cref="InvalidOperationException"></exception>
        public bool IsRecacheRequired([NotNullWhen(false)] out IVersionCache? versionCache)
        {
            _cacheDirectory.CreateCacheDirectoryIfNotExisting();

            var configFile = _options.ConfigFile;
            var gitDirectory = _options.GitDirectory;
            var refsTagDirectory = Path.Combine(gitDirectory, _refsTagDirectory);
            var refsHeadDirectory = Path.Combine(gitDirectory, _refsHeadsDirectory);
            var packedRefsFile = Path.Combine(gitDirectory, _packedRefsDirectory);
            var hashFile = Path.Combine(_cacheDirectory.CacheDirectoryPath, $"{_cacheManager.CacheId}{_hashExtension}");

            var upToDateHashCode = new UpToDateHashCode();

            if (configFile != null) {
                upToDateHashCode.AddFile(configFile);
            }

            if (Directory.Exists(refsTagDirectory)) {
                upToDateHashCode.AddDirectory(refsTagDirectory);
            }

            if (Directory.Exists(refsHeadDirectory)) {
                upToDateHashCode.AddDirectory(refsHeadDirectory);
            }

            if (File.Exists(packedRefsFile)) {
                upToDateHashCode.AddFile(packedRefsFile);
            }

            using var upToDateFileStream = FileStreamLocker.Default.WaitUntilAcquired(hashFile)
                ?? throw new InvalidOperationException($"Cannot acquire lock on up-to-date file: {hashFile}");

            using var upToDateFileBuffer = new MemoryStream((int)upToDateFileStream.Length);
            upToDateFileStream.CopyTo(upToDateFileBuffer);

            var upToDateHashCodeBytes = upToDateHashCode.ToHashCode();
            var upToDateFileHashCode = upToDateFileBuffer.ToArray();
            var isUpToDateHashEqual = upToDateFileHashCode.SequenceEqual(upToDateHashCodeBytes);

            bool isCacheUpToDate;

            if (isUpToDateHashEqual && !_cacheManager.IsRecacheRequired(out versionCache)) {
                isCacheUpToDate = true;
            } else {
                isCacheUpToDate = false;
                versionCache = null;

                if (!isUpToDateHashEqual) {
                    upToDateFileStream.SetLength(0);
                    upToDateFileStream.Flush();

                    using var binaryWriter = new BinaryWriter(upToDateFileStream);
                    binaryWriter.Write(upToDateHashCodeBytes);
                }
            }

            _logger.LogInformation("Checked version cache (Cache id = {CacheId}, Up-to-date = {UpToDate})", _cacheManager.CacheId, isCacheUpToDate);
            return !isCacheUpToDate;
        }
    }
}
