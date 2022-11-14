using K4os.Hash.xxHash;

namespace Vernuntii.Cryptography
{
    /// <summary>
    /// Calculates a hash of multiple _files.
    /// </summary>
    public class UpToDateHashCode
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
        /// Writes hashcodes of unique and ordered files to a byte array.
        /// </summary>
        public byte[] ToHashCode()
        {
            var hashAlgorithm = new XXH64();

            foreach (var file in new SortedSet<string>(_files, StringComparer.InvariantCulture)) {
                try {
                    hashAlgorithm.Update(File.ReadAllBytes(file));
                } catch {
                    // Can safely be ignored.
                }
            }

            return hashAlgorithm.DigestBytes();
        }
    }
}
