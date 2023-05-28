using Vernuntii.Reactive.Coroutines.Steps;

namespace Vernuntii.Reactive.Coroutines;

internal class EventConnector<T> : IEventConnector
{
    public IEventTrace Trace => _trace;

    private EventTrace<T> _trace;
    private readonly IObservableEvent<T> _observableEvent;

    public EventConnector(EventTrace<T> trace, IObservableEvent<T> observableEvent)
    {
        _trace = trace;
        _observableEvent = observableEvent;
    }

    public IEventConnection Connect() =>
        new EventConnection<T>(_trace, _observableEvent);
}
