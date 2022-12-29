using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive;

internal class EveryEvent<T> : IEventDataHolder<T>, IObservableEvent<T>, IUnschedulableEventObserver<T>
{
    [MaybeNull]
    internal virtual T EventData => throw new InvalidOperationException();

    internal virtual bool HasEventData => false;
    internal virtual bool IsCompleted => false;

    [MaybeNull]
    T IEventDataHolder<T>.EventData => EventData;

    bool IEventDataHolder<T>.HasEventData => HasEventData;

    protected bool HasEventEntries => _eventEntries.Count != 0;

    private readonly SortedSet<EventObserverEntry> _eventEntries = new();

    protected virtual IEventDataHolder<T> CanEvaluate(T eventData) =>
        new EventDataHolder<T>(eventData, hasEventData: true);

    protected void Fullfill(EventFulfillmentContext context, T eventData)
    {
        foreach (var eventEntry in _eventEntries) {
            if (eventEntry.Handler.IsUnschedulable) {
                eventEntry.Handler.OnFulfilled(context, eventData);

            } else {
                context.ScheduleFulfillment(eventEntry.Handler, eventData);
            }
        }
    }

    protected virtual void PostEvaluation(EventFulfillmentContext context, T eventData) =>
        Fullfill(context, eventData);

    internal void Evaluate(EventFulfillmentContext context, T eventData)
    {
        var result = CanEvaluate(eventData);

        if (!result.HasEventData) {
            return;
        }

        PostEvaluation(context, eventData);
    }

    void IUnschedulableEventObserver<T>.OnFulfilled(EventFulfillmentContext context, T eventData) =>
        Evaluate(context, eventData);

    public virtual IDisposable Subscribe(IEventObserver<T> observer)
    {
        var eventEntry = new EventObserverEntry(observer);
        _eventEntries.Add(eventEntry);

        return new DelegatingDisposable<(ISet<EventObserverEntry>, EventObserverEntry)>(
            static result => result.Item1.Remove(result.Item2),
            (_eventEntries, eventEntry));
    }

    protected readonly record struct EventObserverEntry : IComparable<EventObserverEntry>
    {
        private static uint s_nextId;

        public uint Id { get; }
        public IEventObserver<T> Handler =>
            _eventObserver ?? throw new InvalidOperationException();

        private readonly IEventObserver<T>? _eventObserver;

        public EventObserverEntry(IEventObserver<T> eventObserver)
        {
            Id = Interlocked.Increment(ref s_nextId);
            _eventObserver = eventObserver;
        }

        public int CompareTo(EventObserverEntry other) =>
            Id.CompareTo(other.Id);
    }
}
