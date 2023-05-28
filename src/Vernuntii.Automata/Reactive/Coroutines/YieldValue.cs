namespace Vernuntii.Reactive.Coroutines;

internal class YieldValue<T>
{
    public T Value {
        get {
            if (!HasValue) {
                throw new InvalidOperationException();
            }

            return _value;
        }

        set {
            _value = value;
            HasValue = true;
        }
    }

    internal bool HasValue { get; private set; }

    private T _value = default!;
}
