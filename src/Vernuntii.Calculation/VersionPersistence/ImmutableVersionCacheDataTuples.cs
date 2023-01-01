using System.Diagnostics.CodeAnalysis;
using Microsoft.Collections.Extensions;

namespace Vernuntii.VersionPersistence;

internal sealed class ImmutableVersionCacheDataTuples : IImmutableVersionCacheDataTuples
{
    public static readonly ImmutableVersionCacheDataTuples Empty = new(new OrderedDictionary<VersionCachePartWithType, object?>(capacity: 0));

    private OrderedDictionary<VersionCachePartWithType, object?> _dataTuples;

    private ImmutableVersionCacheDataTuples(OrderedDictionary<VersionCachePartWithType, object?> dataTuples) =>
        _dataTuples = dataTuples;

    public ImmutableVersionCacheDataTuples(VersionCacheDataTuples dataTuples) =>
        _dataTuples = new OrderedDictionary<VersionCachePartWithType, object?>(dataTuples._dataTuples);

    public bool TryGetData(IVersionCachePart part, out object? data, [NotNullWhen(true)] out Type? dataType)
    {
        if (_dataTuples.Count == 0) {
            data = null;
            dataType = null;
            return false;
        }

        var index = _dataTuples.IndexOf(VersionCachePartWithType.WithoutType(part.Name));

        if (index == -1) {
            data = null;
            dataType = null;
            return false;
        }

        var partDataTuple = ((IList<KeyValuePair<VersionCachePartWithType, object>>)_dataTuples)[index];
        data = partDataTuple.Value;
        dataType = partDataTuple.Key.Type;
        return true;
    }
}
