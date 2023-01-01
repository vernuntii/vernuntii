using System.Collections;
using System.Collections.Immutable;
using Vernuntii.Collections;

namespace Vernuntii.VersionPersistence.Presentation;

public class VersionPresentationParts : IReadOnlySet<VersionCachePart>, IReadOnlyContentwiseCollection<VersionCachePart>
{
    public static VersionPresentationParts Of(params VersionCachePart[] parts) =>
        new VersionPresentationParts(parts);

    public static readonly VersionPresentationParts Empty =
        new VersionPresentationParts(ImmutableHashSet<VersionCachePart>.Empty);

    public int Count => _parts.Count;

    private IImmutableSet<VersionCachePart> _parts;

    private VersionPresentationParts(IImmutableSet<VersionCachePart> parts) =>
        _parts = parts;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="parts"></param>
    public VersionPresentationParts(IEnumerable<VersionCachePart> parts) =>
        _parts = parts.ToImmutableHashSet();

    /// <inheritdoc/>
    public bool Contains(VersionCachePart item) =>
        _parts.Contains(item);

    public bool TryGetValue(VersionCachePart value1, out VersionCachePart value2) =>
        _parts.TryGetValue(value1, out value2);

    /// <inheritdoc/>
    public bool IsProperSubsetOf(IEnumerable<VersionCachePart> other) =>
        _parts.IsProperSubsetOf(other);

    /// <inheritdoc/>
    public bool IsProperSupersetOf(IEnumerable<VersionCachePart> other) =>
        _parts.IsProperSupersetOf(other);
    /// <inheritdoc/>
    public bool IsSubsetOf(IEnumerable<VersionCachePart> other) =>
        _parts.IsSubsetOf(other);

    /// <inheritdoc/>
    public bool IsSupersetOf(IEnumerable<VersionCachePart> other) =>
        _parts.IsSupersetOf(other);

    /// <inheritdoc/>
    public bool Overlaps(IEnumerable<VersionCachePart> other) =>
        _parts.Overlaps(other);

    /// <inheritdoc/>
    public bool SetEquals(IEnumerable<VersionCachePart> other) =>
        _parts.SetEquals(other);

    public IEnumerator<VersionCachePart> GetEnumerator() =>
        _parts.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable)_parts).GetEnumerator();
}
