using System.Buffers;

namespace Vernuntii.PluginSystem.Reactive;

internal record EventSystem : IEventSystem
{
    private readonly Dictionary<object, EventObserverCollection> _eventObservers = new();
    private readonly object _lock = new();

    internal EventSystem()
    {
    }

    private EventChain<T> CreateEventChain<T>(EventChainFragment<T> fragment) =>
        new EventChain<T>(this, fragment);

    EventChain<T> IEventChainFactory.Create<T>(EventChainFragment<T> fragment) =>
        CreateEventChain(fragment);

    internal IDisposable AddObserver(object eventId, ITypeInversedUnschedulableEventFulfiller eventObserver)
    {
        lock (_lock) {
            if (!_eventObservers.TryGetValue(eventId, out var eventObservers)) {
                eventObservers = new EventObserverCollection();
                _eventObservers[eventId] = eventObservers;
            }

            return eventObservers.Add(eventObserver);
        }
    }

    internal virtual async Task FulfillScheduledEventsAsync<T>(object eventId, T eventData, EventFulfillmentContext fulfillmentContext)
    {
        foreach (var scheduledEventInvocation in fulfillmentContext.ScheduledEventFulfillments) {
            var task = scheduledEventInvocation.Item1(scheduledEventInvocation.Item2);

            if (!task.IsCompletedSuccessfully) {
                await task.ConfigureAwait(false);
            }
        }
    }

    public Task FullfillAsync<T>(object eventId, T eventData)
    {
        var fulfillmentContext = new EventFulfillmentContext();

        lock (_lock) {
            if (!_eventObservers.TryGetValue(eventId, out var eventHandlers)) {
                return Task.CompletedTask;
            }

            eventHandlers.Fulfill(fulfillmentContext, eventData);
        }

        return FulfillScheduledEventsAsync(eventId, eventData, fulfillmentContext);
    }

    private class EventObserverCollection
    {
        private static ulong s_nextId;

        private readonly SortedSet<EventObserverEntry> _eventObserverEntries = new();

        public IDisposable Add(ITypeInversedUnschedulableEventFulfiller eventHandler)
        {
            var entries = _eventObserverEntries;
            var entry = new EventObserverEntry(Interlocked.Increment(ref s_nextId), eventHandler);

            lock (entries) {
                entries.Add(entry);
            }

            return DelegatingDisposable.Create(
                state => {
                    var (entries, entry) = state;

                    lock (entries) {
                        entries.Remove(entry);

                        if (entries.Count == 0) {
                            // Reset id
                            s_nextId = 0;
                        }
                    }
                },
                (entries, entry));
        }

        public void Fulfill<T>(EventFulfillmentContext context, T eventData)
        {
            var entriesCount = _eventObserverEntries.Count;
            var entries = ArrayPool<EventObserverEntry>.Shared.Rent(entriesCount);

            try {
                _eventObserverEntries.CopyTo(entries, 0, entriesCount);

                for (var i = 0; i < entriesCount; i++) {
                    entries[i].Handler.Fulfill(context, eventData);
                }
            } finally {
                ArrayPool<EventObserverEntry>.Shared.Return(entries);
            }
        }

        protected readonly record struct EventObserverEntry : IComparable<EventObserverEntry>
        {
            public ulong Id { get; }

            public ITypeInversedUnschedulableEventFulfiller Handler =>
                _eventHandler ?? throw new InvalidOperationException();

            private readonly ITypeInversedUnschedulableEventFulfiller? _eventHandler;

            public EventObserverEntry(ulong id, ITypeInversedUnschedulableEventFulfiller eventHandler)
            {
                Id = id;
                _eventHandler = eventHandler;
            }

            public int CompareTo(EventObserverEntry other) =>
                Id.CompareTo(other.Id);
        }
    }
}


