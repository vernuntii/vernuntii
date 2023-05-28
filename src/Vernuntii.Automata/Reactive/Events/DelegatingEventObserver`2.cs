namespace Vernuntii.Reactive.Events;

internal class DelegatingEventObserver<T, TState> : IEventObserver<T>
{
    internal delegate Task HandleEventDelegate(in Tuple tuple);

    private readonly HandleEventDelegate _eventObserver;
    private readonly TState _state;

    public DelegatingEventObserver(HandleEventDelegate eventObserver, TState state)
    {
        _eventObserver = eventObserver ?? throw new ArgumentNullException(nameof(eventObserver));
        _state = state;
    }

    public Task OnEmissionAsync(T eventData) =>
        _eventObserver(new Tuple(eventData, _state));

    internal readonly struct Tuple
    {
        public T EventData { get; }
        public TState State { get; }

        internal Tuple(T eventData, TState state)
        {
            EventData = eventData;
            State = state;
        }
    }
}
