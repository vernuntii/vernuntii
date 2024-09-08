namespace Vernuntii.Coroutines;

public class CoroutineContextServiceMap : AbstractRobinhoodMap<Key, object>
{
    private static readonly ReadonlyReferenceEqualsDelegate<Key> ReadOnlyReferenceEqualsDelegate = KeyEqualityComparer.Equals;
    private static readonly ReadonlyReferenceGetHashCodeDelegate<Key> ReadOnlyReferenceGetHashCodeDelegate = KeyEqualityComparer.GetHashCode;

    public static CoroutineContextServiceMap CreateRange<TState>(int length, TState state, Action<CoroutineContextServiceMap, TState> entriesBuilder)
    {
        var dictionary = new CoroutineContextServiceMap((uint)length);
        entriesBuilder(dictionary, state);
        return dictionary;
    }

    public CoroutineContextServiceMap() : base(4, DefaultLoadFactor, ReadOnlyReferenceEqualsDelegate, ReadOnlyReferenceGetHashCodeDelegate)
    {
    }

    public CoroutineContextServiceMap(uint length) : base(length, DefaultLoadFactor, ReadOnlyReferenceEqualsDelegate, ReadOnlyReferenceGetHashCodeDelegate)
    {
    }
}
