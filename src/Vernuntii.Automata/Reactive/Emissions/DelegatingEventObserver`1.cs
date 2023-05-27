namespace Vernuntii.Reactive.Emissions;

internal class DelegatingEventObserver<T> : IEventObserver<T>
{
    private readonly Func<T, Task> _eventObserver;

    public DelegatingEventObserver(Func<T, Task> eventObserver) =>
        _eventObserver = eventObserver ?? throw new ArgumentNullException(nameof(eventObserver));

    public Task OnEmissionAsync(T eventData) =>
        _eventObserver(eventData);

}
