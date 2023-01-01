using Vernuntii.VersionPersistence.Serialization;

namespace Vernuntii.VersionPersistence;

public sealed class VersionCacheManagerContext
{
    /// <summary>
    /// Allows to add further serialization semantics when the cache is going to be written to disk.
    /// </summary>
    /// <remarks>
    /// Please keep in mind, that the order of write operations of formatters must match previous read
    /// operations of deformatters and vice versa, otherwise a <see cref="VersionCacheSerializationException"/>
    /// exception can be thrown and may lead to an inevitable recache.
    /// </remarks>
    public Dictionary<object, VersionCacheFormatterTuple> Serializers { get; } = new();
}
