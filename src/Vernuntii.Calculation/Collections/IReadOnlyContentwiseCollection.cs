using System.Collections.Immutable;

namespace Vernuntii.Collections;

public interface IReadOnlyContentwiseCollection<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// Determines if the set contains a specific item.
    /// </summary>
    /// <param name="value">The item to check if the collection contains.</param>
    /// <returns><see langword="true"/> if found; otherwise <see langword="false"/>.</returns>
    bool Contains(T value);

    /// <inheritdoc cref="IImmutableSet{T}.TryGetValue(T, out T)"/>
    bool TryGetValue(T value1, out T value2);
}
