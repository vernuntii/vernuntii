namespace Vernuntii.Reactive.Events;

internal class DelegatingBacklogBackedEventObserver<T, TState> : IBacklogBackedEventObserver<T>
{
    internal delegate void HandleEventDelegate(in Arguments args);

    private readonly HandleEventDelegate _eventObserver;
    private readonly TState _state;

    public DelegatingBacklogBackedEventObserver(HandleEventDelegate eventObserver, TState state)
    {
        _eventObserver = eventObserver ?? throw new ArgumentNullException(nameof(eventObserver));
        _state = state;
    }

    public void OnEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        _eventObserver(new Arguments(emissionBacklog, eventData, _state));

    internal readonly record struct Arguments(EventEmissionBacklog EmissionBacklog, T EventData, TState State);
}
