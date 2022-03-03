using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionFoundation.Caching
{
    public interface ISemanticVersionFoundationCache<T>
        where T : class
    {
        void DeleteCacheFiles(string gitDirectory);

        bool TryGetCache(
            string gitDirectory,
            string cacheId,
            [NotNullWhen(true)] out T? presentationFoundation,
            out ISemanticVersionFoundationWriter<T> versionPresentationFoundationWriter);
    }
}
