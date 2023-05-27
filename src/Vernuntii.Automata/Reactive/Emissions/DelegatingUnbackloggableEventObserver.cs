namespace Vernuntii.Reactive.Emissions;

internal class DelegatingUnbackloggableEventObserver<T, TState> : IUnbackloggableEventObserver<T>
{
    internal delegate void HandleEventDelegate(in Tuple tuple);

    private readonly HandleEventDelegate _eventObserver;
    private readonly TState _state;

    public DelegatingUnbackloggableEventObserver(HandleEventDelegate eventObserver, TState state)
    {
        _eventObserver = eventObserver ?? throw new ArgumentNullException(nameof(eventObserver));
        _state = state;
    }

    public void OnEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        _eventObserver(new Tuple(emissionBacklog, eventData, _state));

    internal readonly record struct Tuple(EventEmissionBacklog EmissionBacklog, T EventData, TState State);
}
