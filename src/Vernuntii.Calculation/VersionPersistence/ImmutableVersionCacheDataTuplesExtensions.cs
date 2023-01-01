using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

public static class ImmutableVersionCacheDataTuplesExtensions
{
    public static bool TryGetData<T>(this IImmutableVersionCacheDataTuples dataTuples, VersionCachePartWithType<T> part, [NotNullWhen(true)] out T? data)
    {
        if (dataTuples.TryGetData(part, out var plainData, out _)) {
            data = (T)plainData!;
            return true;
        }

        data = default;
        return false;
    }

    public static T? GetDataOrDefault<T>(this IImmutableVersionCacheDataTuples dataTuples, VersionCachePartWithType<T> part)
    {
        if (dataTuples.TryGetData(part, out var plainData, out _)) {
            return (T?)plainData;
        }

        return default;
    }
}
