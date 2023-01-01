using System.Runtime.CompilerServices;

namespace Vernuntii.VersionPersistence;

/// <summary>
/// Represents a version presentation part.
/// </summary>
public class VersionCachePartWithType : VersionCachePart
{
    /// <summary>
    /// Creates a new version presentation part.
    /// </summary>
    /// <param name="name">A non-null and non-white-space name.</param>
    /// <param name="type"></param>
    public static VersionCachePartWithType New(string name, Type type)
    {
        if (type is null) {
            throw new ArgumentNullException(nameof(type));
        }

        return new VersionCachePartWithType(name, type);
    }

    internal static VersionCachePartWithType WithoutType(string name) =>
        new VersionCachePartWithType(name, type: null);

    /// <inheritdoc cref="New(string,Type)"/>
    internal static VersionCachePartWithType FromCallerMember(Type type, [CallerMemberName] string? name = null) =>
        new VersionCachePartWithType(name!, type);

    /// <inheritdoc cref="New(string,Type)"/>
    internal static VersionCachePartWithType<T> FromCallerMember<T>([CallerMemberName] string? name = null) =>
        new VersionCachePartWithType<T>(name!);

    /// <summary>
    /// The type this version cache part represents.
    /// </summary>
    public Type Type => _type ?? throw new InvalidOperationException("The version cache part is not associated with a type as this instance is only intended for data lookup");

    private readonly Type? _type;

    internal VersionCachePartWithType(string name, Type? type) : base(name) =>
        _type = type;
}
