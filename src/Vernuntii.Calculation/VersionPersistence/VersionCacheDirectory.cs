using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// The cache for the version foundation.
    /// </summary>
    public class VersionCacheDirectory : IVersionCacheDirectory
    {
        //private readonly static FileStreamLocker deleteOnCloseFileLocker = new FileStreamLocker(new DeleteOnCloseLockFileSystem());

        /// <inheritdoc/>
        public string? CacheDirectoryPath { get; private set; }

        private readonly VersionCacheDirectoryOptions _options;
        private readonly string _baseDirectory;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="options"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public VersionCacheDirectory(VersionCacheDirectoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            var baseDirectory = options.BaseDirectory;

            if (!Directory.Exists(baseDirectory)) {
                throw new DirectoryNotFoundException($"The directory does not exist: {baseDirectory}");
            }

            _baseDirectory = baseDirectory;
        }

        //private static bool IsCacheExpired(FileStream fileStream)
        //{
        //    var versionCache = JsonFile<T>.ReadValue(fileStream);
        //    return VersionRecacheIndicator.Default.IsCacheExpiredSinceCreation(versionCache);
        //}

        //private static void DeleteFile(string filePath) =>
        //    // We try to lock again and delete on close.
        //    deleteOnCloseFileLocker.TryAcquire(filePath, FileMode.Open)?.Dispose();

        //private static void MaybeDeleteOtherCacheFiles(string cacheDirectoryPath, string? skippableCacheFileName)
        //{
        //    if (!Directory.Exists(cacheDirectoryPath)) {
        //        return;
        //    }

        //    foreach (var filePath in Directory.EnumerateFiles(cacheDirectoryPath)) {
        //        if (string.Equals(Path.GetFileName(filePath), skippableCacheFileName, StringComparison.OrdinalIgnoreCase)) {
        //            continue;
        //        }

        //        try {
        //            using var fileStream = JsonFile<T>.FileStreamLocker.TryAcquire(filePath, fileMode: FileMode.Open);

        //            if (fileStream != null && IsCacheExpired(fileStream)) {
        //                // Release cacheFile lock.
        //                fileStream.Dispose();
        //                DeleteFile(filePath);
        //            }
        //        } catch (FileNotFoundException) {
        //            // Ignored intentionally.
        //        }
        //    }
        //}

        private string GetCacheDirectoryPath() =>
            Path.Combine(_baseDirectory, _options.CacheFolderName);

        ///// <inheritdoc/>
        //public void DeleteCacheFiles()
        //{
        //    var cacheDirectoryPath = GetCacheDirectoryPath();

        //    if (!Directory.Exists(cacheDirectoryPath)) {
        //        return;
        //    }

        //    foreach (var filePath in Directory.EnumerateFiles(cacheDirectoryPath)) {
        //        try {
        //            DeleteFile(filePath);
        //        } catch (FileNotFoundException) {
        //            // Ignored intentionally.
        //        }
        //    }
        //}

        /// <inheritdoc/>
        [MemberNotNull(nameof(CacheDirectoryPath))]
        public void CreateCacheDirectoryIfNotExisting()
        {
            if (CacheDirectoryPath == null) {
                var cacheDirectoryPath = GetCacheDirectoryPath();
                Directory.CreateDirectory(cacheDirectoryPath);
                CacheDirectoryPath = cacheDirectoryPath;
            }
        }

        //private class DeleteOnCloseLockFileSystem : ILockFileSystem
        //{
        //    public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share) =>
        //        new FileStream(path, mode, access, share, bufferSize: 4096, FileOptions.DeleteOnClose);
        //}
    }
}
