namespace Vernuntii.Reactive;

internal class DelegatingUnschedulableEventEmitter<T, TState> : IUnschedulableEventEmitter<T>
{
    internal delegate void HandleEventDelegate(in Tuple tuple);

    private readonly HandleEventDelegate _eventHandler;
    private readonly TState _state;

    public DelegatingUnschedulableEventEmitter(HandleEventDelegate eventHandler, TState state)
    {
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        _state = state;
    }

    public void Emit(EventEmissionContext context, T eventData) =>
        _eventHandler(new Tuple(context, eventData, _state));

    internal readonly struct Tuple
    {
        public EventEmissionContext EmissionContext { get; }
        public T EventData { get; }
        public TState State { get; }

        internal Tuple(EventEmissionContext context, T eventData, TState state)
        {
            EmissionContext = context;
            EventData = eventData;
            State = state;
        }
    }
}
