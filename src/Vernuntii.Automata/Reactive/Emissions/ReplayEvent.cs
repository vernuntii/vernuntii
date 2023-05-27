//namespace Vernuntii.Reactive.Emissions;

//internal class ReplayEvent<T> : EveryEvent<T>
//{
//    private readonly int _capacity;
//    private Queue<T> _queue;

//    public ReplayEvent(int capacity)
//    {
//        if (capacity <= 0) {
//            throw new ArgumentOutOfRangeException(nameof(capacity), "Number of elements cannot fall below one");
//        }

//        _queue = new Queue<T>(capacity: capacity);
//        _capacity = capacity;
//    }

//    protected override void TriggerEmission(EventEmissionBacklog emissionBacklog, T eventData)
//    {
//        lock (_queue) {
//            if (_queue.Count == _capacity) {
//                _queue.Dequeue();
//                _queue.Enqueue(eventData);
//            }
//        }

//        base.TriggerEmission(emissionBacklog, eventData);
//    }

//    public override IDisposable Subscribe(IEventObserver<T> eventObserver)
//    {

//        lock (_queue) {
//            foreach (var replayableEventData in _queue) {
//                    eventObserver.OnEmission(
//            }
//        }

//        return base.Subscribe(eventObserver);
//    }
//}
