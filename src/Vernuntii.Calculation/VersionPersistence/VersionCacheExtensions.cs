using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

public static class VersionCacheExtensions
{
    public static bool TryGetAdditionalData<T>(this IVersionCache dataTuples, VersionCachePartWithType<T> part, [NotNullWhen(true)] out T? data)
    {
        if (dataTuples.TryGetData(part, out var plainData, out _)) {
            data = (T)plainData!;
            return true;
        }

        data = default;
        return false;
    }

    public static T? GetAdditionalDataOrDefault<T>(this IVersionCache dataTuples, VersionCachePartWithType<T> part)
    {
        if (dataTuples.TryGetData(part, out var plainData, out _)) {
            return (T?)plainData;
        }

        return default;
    }

    /// <summary>
    /// Wraps <paramref name="versionCache"/> to only accessing data tuples through
    /// <see cref="IVersionCache.TryGetAdditionalData(IVersionCachePart, out object?, out Type?)"/>.
    /// Use it to skip inbuilt version cache part lookups.
    /// </summary>
    /// <param name="versionCache"></param>
    public static IImmutableVersionCacheDataTuples GetAdditionalDataTuples(this IVersionCache versionCache) =>
        new ImmutableVersionCacheAdditionalDataTuples(versionCache);

    private class ImmutableVersionCacheAdditionalDataTuples : IImmutableVersionCacheDataTuples
    {
        private readonly IVersionCache _versionCache;

        public ImmutableVersionCacheAdditionalDataTuples(IVersionCache versionCache) =>
            _versionCache = versionCache ?? throw new ArgumentNullException(nameof(versionCache));

        public bool TryGetData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType) =>
            _versionCache.TryGetAdditionalData(part, out data, out dataType);
    }
}
