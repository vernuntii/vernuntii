namespace Vernuntii.Reactive.Coroutines;

internal class YieldResult<T> : IYieldResult<T>
{
    public T Value {
        get => _value.Value;
        set => _value.Value = value;
    }

    private YieldValue<T> _value = new();
}
