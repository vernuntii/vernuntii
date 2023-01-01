using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionPersistence;

public interface IImmutableVersionCacheDataTuples
{
    /// <summary>
    /// Tries to get data by specifying <paramref name="part"/>.
    /// </summary>
    /// <param name="part"></param>
    /// <param name="data"></param>
    /// <param name="dataType"></param>
    bool TryGetData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType);
}
