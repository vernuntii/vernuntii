namespace Vernuntii.Reactive;

internal class DelegatingUnschedulableEventObserver<T, TState> : IUnschedulableEventFulfiller<T>
{
    internal delegate ValueTask HandleEventDelegate(in Tuple tuple);

    private readonly HandleEventDelegate _eventHandler;
    private readonly TState _state;

    public DelegatingUnschedulableEventObserver(HandleEventDelegate eventHandler, TState state)
    {
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        _state = state;
    }

    public void Fulfill(EventFulfillmentContext context, T eventData) =>
        _eventHandler(new Tuple(context, eventData, _state));

    internal readonly struct Tuple
    {
        public EventFulfillmentContext FulfillmentContext { get; }
        public T EventData { get; }
        public TState State { get; }

        internal Tuple(EventFulfillmentContext context, T eventData, TState state)
        {
            FulfillmentContext = context;
            EventData = eventData;
            State = state;
        }
    }
}
