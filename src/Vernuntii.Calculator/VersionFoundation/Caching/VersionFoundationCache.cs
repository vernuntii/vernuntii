using System.Diagnostics.CodeAnalysis;
using Teronis.IO.FileLocking;
using Vernuntii.SemVer;

namespace Vernuntii.VersionFoundation.Caching
{
    internal class VersionFoundationCache<T> : IVersionFoundationCache<T>
        where T : class, IVersionFoundation<SemanticVersion>
    {
        private readonly static FileStreamLocker deleteOnCloseFileLocker = new FileStreamLocker(new DeleteOnCloseLockFileSystem());

        public VersionFoundationCacheOptions _options;

        public VersionFoundationCache(VersionFoundationCacheOptions options) =>
            _options = options ?? throw new ArgumentNullException(nameof(options));

        private static bool IsCacheExpired(FileStream fileStream)
        {
            var presentationFoundation = VersionFoundationFile<T>.ReadPresentationFoundation(fileStream);
            return presentationFoundation.ExpirationTime != null && DateTime.UtcNow > presentationFoundation.ExpirationTime;
        }

        private static void DeleteFile(string filePath) =>
            // We try to lock again and delete on close.
            deleteOnCloseFileLocker.TryAcquire(filePath, FileMode.Open)?.Dispose();

        private static void MaybeDeleteOtherCacheFiles(string cacheDirectoryPath, string? skippableCacheFileName)
        {
            foreach (var filePath in Directory.EnumerateFiles(cacheDirectoryPath)) {
                if (string.Equals(Path.GetFileName(filePath), skippableCacheFileName, StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                try {
                    using var fileStream = VersionFoundationFile<T>.FileStreamLocker.TryAcquire(filePath, fileMode: FileMode.Open);

                    if (fileStream != null && IsCacheExpired(fileStream)) {
                        // Release file lock.
                        fileStream.Dispose();
                        DeleteFile(filePath);
                    }
                } catch (FileNotFoundException) {
                    // Ignored intentionally.
                }
            }
        }

        private string GetCacheDirectoryPath(string gitDirectory) =>
            Path.Combine(gitDirectory, _options.CacheFolderName);

        public void DeleteCacheFiles(string gitDirectory)
        {
            var cacheDirectoryPath = GetCacheDirectoryPath(gitDirectory);

            foreach (var filePath in Directory.EnumerateFiles(cacheDirectoryPath)) {
                try {
                    DeleteFile(filePath);
                } catch (FileNotFoundException) {
                    // Ignored intentionally.
                }
            }
        }

        public bool TryGetCache(
            string gitDirectory,
            string cacheId,
            [NotNullWhen(true)] out T? presentationFoundation,
            out IVersionFoundationWriter<T> versionPresentationFoundationWriter)
        {
            var cacheDirectoryPath = GetCacheDirectoryPath(gitDirectory);
            Directory.CreateDirectory(cacheDirectoryPath); // Create if not existing.
            var cacheFileName = cacheId + ".json";
            var cacheFilePath = Path.Combine(cacheDirectoryPath, cacheFileName);
            var file = new VersionFoundationFile<T>(cacheFilePath, _options.LockAttemptSeconds);
            MaybeDeleteOtherCacheFiles(cacheDirectoryPath, skippableCacheFileName: cacheFileName);
            versionPresentationFoundationWriter = file;

            try {
                if (file.TryReadPresentationFoundation(out presentationFoundation)) {
                    return true;
                }

                return false;
            } catch {
                file.Dispose();
                throw;
            }
        }

        private class DeleteOnCloseLockFileSystem : ILockFileSystem
        {
            public FileStream Open(string path, FileMode mode, FileAccess access, FileShare share) =>
                new FileStream(path, mode, access, share, bufferSize: 4096, FileOptions.DeleteOnClose);
        }
    }
}
