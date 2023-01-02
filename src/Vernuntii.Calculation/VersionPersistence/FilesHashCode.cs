using System.Buffers;
using K4os.Hash.xxHash;

namespace Vernuntii.VersionPersistence
{
    /// <summary>
    /// Calculates a hash of multiple _files.
    /// </summary>
    public class FilesHashCode
    {
        private readonly List<string> _files = new();

        /// <summary>
        /// Adds files in a directory to check.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="recursively"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void AddDirectory(string directoryPath, bool recursively = false)
        {
            if (!Directory.Exists(directoryPath)) {
                throw new DirectoryNotFoundException($"Directory does not exist: {directoryPath}");
            }

            _files.AddRange(Directory.EnumerateFiles(directoryPath, "*", new EnumerationOptions() {
                RecurseSubdirectories = recursively
            }));
        }

        /// <summary>
        /// Adds a single file to check.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void AddFile(string filePath)
        {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"Directory does not exist: {filePath}");
            }

            _files.Add(filePath);
        }

        /// <summary>
        /// Generates a hash code that represents the ordered files you added so far.
        /// </summary>
        public async Task<byte[]> ToHashCodeAsync()
        {
            var readFileTasks = new SortedSet<string>(_files, StringComparer.InvariantCulture).Select(x => Task.Run(async () => {
                const int bufferSize = 1024 * 1024; // 1 MB
                var hashAlgorithm = new XXH64();
                using var stream = File.OpenRead(x);
                using var memoryOwner = MemoryPool<byte>.Shared.Rent(bufferSize);
                int size;

                while ((size = await stream.ReadAsync(memoryOwner.Memory).ConfigureAwait(false)) != 0) {
                    hashAlgorithm.Update(memoryOwner.Memory.Span[..size]);
                }

                return hashAlgorithm.DigestBytes();
            }));

            var hashAlgorithm = new XXH64();

            foreach (var readFileTask in readFileTasks) {
                hashAlgorithm.Update(await readFileTask.ConfigureAwait(false));
            }

            return hashAlgorithm.DigestBytes();
        }
    }
}
