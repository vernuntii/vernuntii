using System.Buffers;

namespace Vernuntii.Reactive.Centralized;

public record EventStore : IEventStore
{
    private readonly Dictionary<object, EventObserverCollection> _eventObservers = new();
    private readonly object _lock = new();

    public EventStore()
    {
    }

    private EventChain<T> CreateEventChain<T>(EventChainFragment<T> fragment) =>
        new EventChain<T>(this, fragment);

    EventChain<T> IEventChainability.Chain<T>(EventChainFragment<T> fragment) =>
        CreateEventChain(fragment);

    internal IDisposable AddObserver(EventId eventId, ITypeInversedChainableEventObserver eventObserver)
    {
        lock (_lock) {
            if (!_eventObservers.TryGetValue(eventId, out var eventObservers)) {
                eventObservers = new EventObserverCollection();
                _eventObservers[eventId] = eventObservers;
            }

            return eventObservers.Add(eventObserver);
        }
    }

    internal virtual async Task EmitBacklogAsync<T>(EventId eventId, T eventData, EventEmissionBacklog emissionBacklog)
    {
        foreach (var scheduledEventEmissions in emissionBacklog.Collection) {
            var task = scheduledEventEmissions.Item1(scheduledEventEmissions.Item2);

            if (!task.IsCompletedSuccessfully) {
                await task.ConfigureAwait(false);
            }
        }
    }

    public Task EmitAsync<T>(EventId eventId, T eventData)
    {
        var emissionBacklog = new EventEmissionBacklog();

        lock (_lock) {
            if (!_eventObservers.TryGetValue(eventId, out var eventObservers)) {
                return Task.CompletedTask;
            }

            eventObservers.Emit(emissionBacklog, eventData);
        }

        return EmitBacklogAsync(eventId, eventData, emissionBacklog);
    }

    private class EventObserverCollection
    {
        private static ulong s_nextId;

        private readonly SortedSet<EventObserverEntry> _eventObserverEntries = new();

        public IDisposable Add(ITypeInversedChainableEventObserver eventObserver)
        {
            var entries = _eventObserverEntries;
            var entry = new EventObserverEntry(Interlocked.Increment(ref s_nextId), eventObserver);

            lock (entries) {
                entries.Add(entry);
            }

            return DelegatingDisposable.Create(
                state => {
                    var (entries, entry) = state;

                    lock (entries) {
                        entries.Remove(entry);

                        // BUG:
                        //if (entries.Count == 0) {
                        //    // Reset id
                        //    s_nextId = 0;
                        //}
                    }
                },
                (entries, entry));
        }

        public void Emit<T>(EventEmissionBacklog emissionBacklog, T eventData)
        {
            var entriesCount = _eventObserverEntries.Count;
            var entries = ArrayPool<EventObserverEntry>.Shared.Rent(entriesCount);

            try {
                _eventObserverEntries.CopyTo(entries, 0, entriesCount);

                for (var i = 0; i < entriesCount; i++) {
                    entries[i].Handler.OnEmission(emissionBacklog, eventData);
                }
            } finally {
                ArrayPool<EventObserverEntry>.Shared.Return(entries);
            }
        }

        protected readonly record struct EventObserverEntry : IComparable<EventObserverEntry>
        {
            public ulong Id { get; }

            public ITypeInversedChainableEventObserver Handler =>
                _eventObserver ?? throw new InvalidOperationException();

            private readonly ITypeInversedChainableEventObserver? _eventObserver;

            public EventObserverEntry(ulong id, ITypeInversedChainableEventObserver eventObserver)
            {
                Id = id;
                _eventObserver = eventObserver;
            }

            public int CompareTo(EventObserverEntry other) =>
                Id.CompareTo(other.Id);
        }
    }
}


