using K4os.Hash.xxHash;

namespace Vernuntii.VersionPersistence;

internal static class HashAlgorithmExtensions
{
    internal static void Update(this XXH64 hashAlgorithm, IEnumerable<ReadOnlyMemory<byte>> bytes)
    {
        foreach (var file in bytes) {
            try {
                hashAlgorithm.Update(file.Span);
            } catch {
                // Can safely be ignored.
            }
        }
    }
}
