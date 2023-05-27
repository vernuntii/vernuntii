namespace Vernuntii.Reactive.Emissions;

[Obsolete]
internal class ReplayEvent<T> : Event<T>, IObservableEvent<T>
{
    private readonly int _capacity;
    private readonly Queue<T> _queue;

    bool IObservableEvent<T>.UseEmissionBacklog => true;

    public ReplayEvent(int capacity)
    {
        if (capacity <= 0) {
            throw new ArgumentOutOfRangeException(nameof(capacity), "The number of possible replay event datas should be greater than zero");
        }

        _queue = new Queue<T>(capacity: capacity);
        _capacity = capacity;
    }

    protected override void TriggerEmission(EventEmissionBacklog emissionBacklog, T eventData)
    {
        lock (_queue) {
            if (_queue.Count == _capacity) {
                _queue.Dequeue();
                _queue.Enqueue(eventData);
            }
        }

        base.TriggerEmission(emissionBacklog, eventData);
    }

    IDisposable IObservableEvent<T>.Subscribe(EventEmissionBacklog emissionBacklog, IEventObserver<T> eventObserver)
    {
        lock (_queue) {
            foreach (var replayEventData in _queue) {
                eventObserver.OnEmissionOrBacklog(emissionBacklog, replayEventData);
            }
        }

        return Subscribe(eventObserver);
    }
}
