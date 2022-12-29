using System.Buffers;

namespace Vernuntii.PluginSystem.Reactive;

public sealed class EventSystem : IEventChainFactory
{
    EventSystem IEventChainFactory.EventSystem => this;

    private readonly Dictionary<ulong, EventObserverCollection> _eventObservers = new();
    private readonly object _lock = new();

    internal IDisposable AddObserver(ulong eventId, IEventObserver eventObserver)
    {
        lock (_lock) {
            if (!_eventObservers.TryGetValue(eventId, out var eventObservers)) {
                eventObservers = new EventObserverCollection();
                _eventObservers[eventId] = eventObservers;
            }

            return eventObservers.Add(eventObserver);
        }
    }

    public async ValueTask FullfillAsync<T>(ulong eventId, T eventData)
    {
        var fulfillmentContext = new EventFulfillmentContext();

        lock (_lock) {
            if (!_eventObservers.TryGetValue(eventId, out var eventHandlers)) {
                return;
            }

            eventHandlers.OnFulfillment(fulfillmentContext, eventData);
        }

        foreach (var scheduledEventInvocation in fulfillmentContext.ScheduledEventInvocations) {
            await scheduledEventInvocation.Item1(scheduledEventInvocation.Item2);
        }
    }

    private class EventObserverCollection
    {
        private static ulong s_nextId;

        private readonly SortedSet<EventObserverEntry> _eventObserverEntries = new();

        public IDisposable Add(IEventObserver eventHandler)
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

        public void OnFulfillment<T>(EventFulfillmentContext context, T eventData)
        {
            var entriesCount = _eventObserverEntries.Count;
            var entries = ArrayPool<EventObserverEntry>.Shared.Rent(entriesCount);

            try {
                _eventObserverEntries.CopyTo(entries, 0, entriesCount);

                for (var i = 0; i < entriesCount; i++) {
                    entries[i].Handler.OnFulfillment(context, eventData);
                }
            } finally {
                ArrayPool<EventObserverEntry>.Shared.Return(entries);
            }
        }

        protected readonly record struct EventObserverEntry : IComparable<EventObserverEntry>
        {
            public ulong Id { get; }

            public IEventObserver Handler =>
                _eventHandler ?? throw new InvalidOperationException();

            private readonly IEventObserver? _eventHandler;

            public EventObserverEntry(ulong id, IEventObserver eventHandler)
            {
                Id = id;
                _eventHandler = eventHandler;
            }

            public int CompareTo(EventObserverEntry other) =>
                Id.CompareTo(other.Id);
        }
    }
}


