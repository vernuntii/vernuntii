namespace Vernuntii.Reactive;

internal class DelegatingEventObserver<T> : IEventObserver<T>
{
    private readonly Func<T, ValueTask> _eventHandler;

    public DelegatingEventObserver(Func<T, ValueTask> eventHandler) =>
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    public ValueTask OnFulfilled(T eventData) =>
        _eventHandler(eventData);

}
