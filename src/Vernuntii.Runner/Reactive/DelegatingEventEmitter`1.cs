namespace Vernuntii.Reactive;

internal class DelegatingEventEmitter<T> : IEventEmitter<T>
{
    private readonly Func<T, Task> _eventHandler;

    public DelegatingEventEmitter(Func<T, Task> eventHandler) =>
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));

    public Task EmitAsync(T eventData) =>
        _eventHandler(eventData);

}
