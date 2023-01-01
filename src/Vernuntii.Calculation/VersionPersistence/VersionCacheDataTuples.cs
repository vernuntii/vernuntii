namespace Vernuntii.VersionPersistence;

public record VersionCacheDataTuples
{
    internal Dictionary<VersionCachePartWithType, object?> _dataTuples = new();

    public void AddData(VersionCachePartWithType part, object? data) =>
        _dataTuples.Add(part, data);
}
