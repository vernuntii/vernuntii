namespace Vernuntii.Coroutines;

public class CoroutineContextServices : RobinhoodMap<Key, object>
{
    private const double s_defaultLoadFactor = 0.5d;

    public static CoroutineContextServices CreateRange<TState>(int length, TState state, Action<CoroutineContextServices, TState> entriesBuilder)
    {
        var dictionary = new CoroutineContextServices((uint)length);
        entriesBuilder(dictionary, state);
        return dictionary;
    }

    public CoroutineContextServices() : base(4, s_defaultLoadFactor, KeyEqualityComparer.Default)
    {
    }

    public CoroutineContextServices(uint length) : base(length, s_defaultLoadFactor, KeyEqualityComparer.Default)
    {
    }
}
