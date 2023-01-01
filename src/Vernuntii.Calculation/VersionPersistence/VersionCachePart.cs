using System.Runtime.CompilerServices;

namespace Vernuntii.VersionPersistence;

/// <summary>
/// Represents a version presentation part.
/// </summary>
public class VersionCachePart : IVersionCachePart
{
    /// <summary>
    /// Creates a new version presentation part.
    /// </summary>
    /// <param name="name">A non-null and non-white-space name.</param>
    public static VersionCachePart New(string name) =>
        new VersionCachePart(name);

    /// <inheritdoc cref="New(string)"/>
    internal static VersionCachePart FromCallerMember([CallerMemberName] string? name = null) =>
        new VersionCachePart(name!);

    /// <summary>
    /// The name of the version presentation part.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    /// <param name="name">A non-null and non-white-space name.</param>
    /// <exception cref="ArgumentException"></exception>
    internal VersionCachePart(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
        }

        Name = name;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is IVersionCachePart otherPart && Name.Equals(otherPart.Name, StringComparison.InvariantCulture);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        Name.GetHashCode(StringComparison.InvariantCulture);

    /// <inheritdoc/>
    public override string ToString() =>
        Name;
}
