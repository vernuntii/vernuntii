using System.Collections;
using System.Collections.Immutable;
using Vernuntii.Collections;

namespace Vernuntii.VersionPersistence.Presentation;

public class VersionPresentationParts : IReadOnlySet<VersionCachePart>, IReadOnlyContentwiseCollection<VersionCachePart>
{
    /// <summary>
    /// A special part which represents all representable parts.
    /// </summary>
    internal static readonly VersionCachePart s_allPart = VersionCachePart.New("All");

    /// <summary>
    /// A default instance that contains no presentable parts.
    /// </summary>
    public static readonly VersionPresentationParts Empty =
        new VersionPresentationParts(ImmutableHashSet<VersionCachePart>.Empty, ImmutableList<VersionCachePart>.Empty);

    /// <summary>
    /// A default instance that contains 'All'.
    /// </summary>
    internal static readonly VersionPresentationParts s_all =
        new VersionPresentationParts(new[] { s_allPart });

    public static VersionPresentationParts Of(params VersionCachePart[] parts) =>
        new VersionPresentationParts(parts);

    /// <summary>
    /// Creates an instance of <see cref="VersionPresentationParts"/> with <paramref name="parts"/>.
    /// </summary>
    /// <param name="parts"></param>
    /// <returns>
    /// <see cref="Empty"/> if <paramref name="parts"/> is null.
    /// </returns>
    public static VersionPresentationParts Of(IEnumerable<VersionCachePart>? parts) => parts is null
        ? Empty
        : new VersionPresentationParts(parts);

    internal static VersionPresentationParts AllowAll(IEnumerable<VersionCachePart>? parts) => parts is null
        ? s_all
        : new VersionPresentationParts(parts.Append(s_allPart));

    /// <inheritdoc/>
    public int Count =>
        _partList.Count;

    private IImmutableSet<VersionCachePart> _partSet;
    private IImmutableList<VersionCachePart> _partList;

    private VersionPresentationParts(IImmutableSet<VersionCachePart> partSet, IImmutableList<VersionCachePart> partList)
    {
        _partSet = partSet;
        _partList = partList;
    }

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="parts"></param>
    public VersionPresentationParts(IEnumerable<VersionCachePart> parts)
    {
        _partSet = parts.ToImmutableHashSet();
        _partList = parts.ToImmutableList();
    }

    /// <inheritdoc/>
    public bool Contains(VersionCachePart item) =>
        _partSet.Contains(item);

    /// <inheritdoc/>
    public bool TryGetValue(VersionCachePart value1, out VersionCachePart value2) =>
        _partSet.TryGetValue(value1, out value2);

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<VersionCachePart> other) =>
        _partSet.IsProperSubsetOf(other);

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<VersionCachePart> other) =>
        _partSet.IsProperSupersetOf(other);
    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<VersionCachePart> other) =>
        _partSet.IsSubsetOf(other);

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<VersionCachePart> other) =>
        _partSet.IsSupersetOf(other);

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<VersionCachePart> other) =>
        _partSet.Overlaps(other);

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<VersionCachePart> other) =>
        _partSet.SetEquals(other);

    public IEnumerator<VersionCachePart> GetEnumerator() =>
        _partList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable)_partList).GetEnumerator();
}
