using System.Buffers;

namespace Vernuntii.Reactive;

internal class EveryEvent<T> : IEmittableEvent<T>, IUnschedulableEventEmitter<T>
{
    protected bool HasEventEntries =>
        _eventSubscriptions.Count != 0;

    private readonly SortedSet<EventSubscription> _eventSubscriptions = new();

    public virtual IDisposable Subscribe(IEventEmitter<T> eventEmitter)
    {
        var eventSubscription = new EventSubscription(eventEmitter);
        _eventSubscriptions.Add(eventSubscription);

        return new DelegatingDisposable<(ISet<EventSubscription>, EventSubscription)>(
            static result => result.Item1.Remove(result.Item2),
            (_eventSubscriptions, eventSubscription));
    }

    protected void Emit(EventEmissionContext context, T eventData)
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
                context.TriggerOrScheduleEventEmission(eventEntry.Handler, eventData);
            }
        } finally {
            ArrayPool<EventSubscription>.Shared.Return(eventSubscriptions);
        }
    }

    protected virtual void TriggerEmission(EventEmissionContext context, T eventData) =>
        Emit(context, eventData);

    protected virtual IEventDataHolder<T>? InspectEmission(T eventData) =>
        new EventDataHolder<T>(eventData, hasEventData: true);

    internal void EvaluateEmission(EventEmissionContext context, T eventData)
    {
        var result = InspectEmission(eventData);

        if (result is null) {
            return;
        }

        TriggerEmission(context, eventData);
    }

    void IUnschedulableEventEmitter<T>.Emit(EventEmissionContext context, T eventData) =>
        EvaluateEmission(context, eventData);

    protected readonly record struct EventSubscription : IComparable<EventSubscription>
    {
        private static uint s_nextId;

        public uint Id { get; }
        public IEventEmitter<T> Handler =>
            _eventEmitter ?? throw new InvalidOperationException();

        private readonly IEventEmitter<T>? _eventEmitter;

        public EventSubscription(IEventEmitter<T> eventEmitter)
        {
            Id = Interlocked.Increment(ref s_nextId);
            _eventEmitter = eventEmitter;
        }

        public int CompareTo(EventSubscription other) =>
            Id.CompareTo(other.Id);
    }
}
