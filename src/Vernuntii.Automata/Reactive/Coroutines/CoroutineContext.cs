namespace Vernuntii.Reactive.Coroutines;

public ref struct CoroutineContext
{
    internal readonly List<CoroutineDefinition> Coroutines { get; }
    //internal readonly List<IEventConnector> EventConnectors { get; }
    internal Dictionary<CoroutinePropertyKey, object> Properties => _properties ??= new Dictionary<CoroutinePropertyKey, object>();

    private int _currentEventId;
    private Dictionary<CoroutinePropertyKey, object>? _properties;

    private int NextEventId() => _currentEventId++;

    public CoroutineContext()
    {
        Coroutines = new List<CoroutineDefinition>();
        //EventConnectors = new List<IEventConnector>();
    }

    //public EventTrace<T> Trace<T>(IObservableEvent<T> observableEvent)
    //{
    //    var eventAction = new EventTrace<T>(NextEventId());
    //    EventConnectors.Add(new EventConnector<T>(eventAction, observableEvent));
    //    return eventAction;
    //}

    public void Spawn(CoroutineDefinition coroutine) =>
        Coroutines.Add(coroutine);
}
