namespace Vernuntii.Reactive;

internal class DelegatingEventObserver<T, TState> : IEventObserver<T>
{
    internal delegate ValueTask HandleEventDelegate(in Tuple tuple);

    private readonly HandleEventDelegate _eventHandler;
    private readonly TState _state;

    public DelegatingEventObserver(HandleEventDelegate eventHandler, TState state)
    {
        _eventHandler = eventHandler ?? throw new ArgumentNullException(nameof(eventHandler));
        _state = state;
    }

    public ValueTask OnFulfilled(T eventData) =>
        _eventHandler(new Tuple(eventData, _state));

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
