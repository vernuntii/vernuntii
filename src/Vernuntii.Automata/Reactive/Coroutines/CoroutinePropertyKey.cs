using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Coroutines;

internal struct CoroutinePropertyKey
{
    private object _identity;

    public CoroutinePropertyKey(object identity) =>
        _identity = identity;

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        (obj is CoroutinePropertyKey key && Equals(key._identity, _identity))
        || ReferenceEquals(obj, _identity)
        || Equals(obj, _identity);

    public override int GetHashCode() => _identity.GetHashCode();
}
