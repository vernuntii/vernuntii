namespace Vernuntii.VersionPersistence;

public static class VersionCacheDataTuplesExtensions
{
    public static void AddData<T>(this VersionCacheDataTuples dataTuples, VersionCachePartWithType<T> part, T data) =>
        dataTuples.AddData(part, data);
}
