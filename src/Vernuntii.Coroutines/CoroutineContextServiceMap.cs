namespace Vernuntii.Coroutines;

public class CoroutineContextServiceMap : AbstractRobinHoodHashMap<Key, object>
{
    private static readonly ReadonlyReferenceEqualsDelegate<Key> s_readOnlyReferenceEqualsDelegate = KeyEqualityComparer.Equals;
    private static readonly ReadonlyReferenceGetHashCodeDelegate<Key> s_readOnlyReferenceGetHashCodeDelegate = KeyEqualityComparer.GetHashCode;

    internal static CoroutineContextServiceMap CreateRange<TState>(int length, TState state, Action<CoroutineContextServiceMap, TState> builder)
    {
        var dictionary = new CoroutineContextServiceMap((uint)length);
        builder(dictionary, state);
        return dictionary;
    }

    public CoroutineContextServiceMap() : base(4, DefaultLoadFactor, s_readOnlyReferenceEqualsDelegate, s_readOnlyReferenceGetHashCodeDelegate)
    {
    }

    public CoroutineContextServiceMap(uint length) : base(length, DefaultLoadFactor, s_readOnlyReferenceEqualsDelegate, s_readOnlyReferenceGetHashCodeDelegate)
    {
    }
}
