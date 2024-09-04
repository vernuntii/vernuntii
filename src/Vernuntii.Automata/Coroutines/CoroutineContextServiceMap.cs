namespace Vernuntii.Coroutines;

public class CoroutineContextServiceMap : RobinhoodMap<Key, object>
{
    private const double s_defaultLoadFactor = 0.5d;

    public static CoroutineContextServiceMap CreateRange<TState>(int length, TState state, Action<CoroutineContextServiceMap, TState> entriesBuilder)
    {
        var dictionary = new CoroutineContextServiceMap((uint)length);
        entriesBuilder(dictionary, state);
        return dictionary;
    }

    public CoroutineContextServiceMap() : base(4, s_defaultLoadFactor, KeyEqualityComparer.Default)
    {
    }

    public CoroutineContextServiceMap(uint length) : base(length, s_defaultLoadFactor, KeyEqualityComparer.Default)
    {
    }
}
