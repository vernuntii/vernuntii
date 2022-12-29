namespace Vernuntii.Reactive;

internal class DelegatingEventObserver<T> : IEventObserver<T>
{
    private readonly Func<T, Task> _eventHandler;

    public DelegatingEventObserver(Func<T, Task> eventHandler) =>
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    public Task OnFulfilled(T eventData) =>
        _eventHandler(eventData);

}
