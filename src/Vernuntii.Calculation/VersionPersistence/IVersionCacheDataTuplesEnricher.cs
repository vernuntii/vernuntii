namespace Vernuntii.VersionPersistence;

/// <summary>
/// Represents a version cache data enrichter for an instance of <see cref="VersionCacheDataTuples"/>.
/// </summary>
public interface IVersionCacheDataTuplesEnricher
{
    /// <summary>
    /// Enriches the version cache data tuples by custom data.
    /// </summary>
    /// <param name="dataTuples"></param>
    void Enrich(VersionCacheDataTuples dataTuples);
}
