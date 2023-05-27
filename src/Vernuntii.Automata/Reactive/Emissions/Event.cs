using System.Buffers;

namespace Vernuntii.Reactive.Emissions;

internal class Event<T> : IObservableEvent<T>, IUnbackloggableEventObserver<T>
{
    protected bool HasEventEntries =>
        _eventSubscriptions.Count != 0;

    private readonly SortedSet<EventSubscription> _eventSubscriptions = new();

    public virtual IDisposable Subscribe(IEventObserver<T> eventObserver)
    {
        var eventSubscription = new EventSubscription(eventObserver);
        _eventSubscriptions.Add(eventSubscription);

        return new DelegatingDisposable<(ISet<EventSubscription>, EventSubscription)>(
            static result => result.Item1.Remove(result.Item2),
            (_eventSubscriptions, eventSubscription));
    }

    protected void Emit(EventEmissionBacklog emissionBacklog, T eventData)
    {
        var eventSubscriptionsCount = _eventSubscriptions.Count;

        if (eventSubscriptionsCount == 0) {
            return;
        }

        var eventSubscriptions = ArrayPool<EventSubscription>.Shared.Rent(eventSubscriptionsCount);

        try {
            _eventSubscriptions.CopyTo(eventSubscriptions);

            for (var index = 0; index < eventSubscriptionsCount; index++) {
                var eventEntry = eventSubscriptions[index];
                eventEntry.Handler.OnEmissionOrBacklog(emissionBacklog, eventData);
            }
        } finally {
            ArrayPool<EventSubscription>.Shared.Return(eventSubscriptions);
        }
    }

    protected virtual void TriggerEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        Emit(emissionBacklog, eventData);

    protected virtual IEventDataHolder<T>? InspectEmission(T eventData) =>
        new EventDataHolder<T>(eventData, hasEventData: true);

    internal void EvaluateEmission(EventEmissionBacklog emissionBacklog, T eventData)
    {
        var result = InspectEmission(eventData);

        if (result is null) {
            return;
        }

        TriggerEmission(emissionBacklog, eventData);
    }

    void IUnbackloggableEventObserver<T>.OnEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        EvaluateEmission(emissionBacklog, eventData);

    protected readonly record struct EventSubscription : IComparable<EventSubscription>
    {
        private static uint s_nextId;

        public uint Id { get; }
        public IEventObserver<T> Handler =>
            _eventObserver ?? throw new InvalidOperationException();

        private readonly IEventObserver<T>? _eventObserver;

        public EventSubscription(IEventObserver<T> eventObserver)
        {
            Id = Interlocked.Increment(ref s_nextId);
            _eventObserver = eventObserver;
        }

        public int CompareTo(EventSubscription other) =>
            Id.CompareTo(other.Id);
    }
}
