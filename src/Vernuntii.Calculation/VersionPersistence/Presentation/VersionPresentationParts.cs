using System.Collections;
using System.Collections.Immutable;
using Vernuntii.Collections;

namespace Vernuntii.VersionPersistence.Presentation;

public class VersionPresentationParts : IReadOnlySet<VersionCachePart>, IReadOnlyContentwiseCollection<VersionCachePart>
{
    /// <summary>
    /// A special part which represents all representable parts.
    /// </summary>
    private static readonly VersionCachePart s_allPart = VersionCachePart.New("All");

    /// <summary>
    /// A default instance that contains no presentable parts.
    /// </summary>
    public static readonly VersionPresentationParts Empty =
        new VersionPresentationParts(ImmutableHashSet<VersionCachePart>.Empty, ImmutableList<VersionCachePart>.Empty);

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

    internal static VersionPresentationParts AllowAll(IEnumerable<VersionCachePart>? parts, IEqualityComparer<VersionCachePart> equalityComparer) => parts is null
        ? new VersionPresentationParts(new[] { s_allPart }, equalityComparer)
        : new VersionPresentationParts(parts.Append(s_allPart), equalityComparer);

    internal static bool HasAllPart(IEnumerable<VersionCachePart> parts) =>
        parts.Contains(s_allPart);

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
    /// <param name="equalityComparer"></param>
    internal VersionPresentationParts(IEnumerable<VersionCachePart> parts, IEqualityComparer<VersionCachePart> equalityComparer)
    {
        _partSet = parts.ToImmutableHashSet(equalityComparer);
        _partList = parts.ToImmutableList();
    }

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="parts"></param>
    public VersionPresentationParts(IEnumerable<VersionCachePart> parts)
        : this(parts, EqualityComparer<VersionCachePart>.Default) { }

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
