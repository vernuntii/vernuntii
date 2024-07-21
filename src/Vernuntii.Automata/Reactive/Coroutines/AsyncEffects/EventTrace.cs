namespace Vernuntii.Reactive.Coroutines.AsyncEffects;

public sealed record EventTrace<T>() : IEventTrace
{
    public int Id {
        get => _id.Value;
        set => _id.Value = value;
    }

    public bool HasId => _id.HasValue;

    private YieldValue<int> _id = new();
}
