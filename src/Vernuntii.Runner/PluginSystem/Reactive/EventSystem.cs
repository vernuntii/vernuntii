using System.Buffers;

namespace Vernuntii.PluginSystem.Reactive;

internal record EventSystem : IEventSystem
{
    private readonly Dictionary<object, EventEmitterCollection> _eventEmitters = new();
    private readonly object _lock = new();

    internal EventSystem()
    {
    }

    private EventChain<T> CreateEventChain<T>(EventChainFragment<T> fragment) =>
        new EventChain<T>(this, fragment);

    EventChain<T> IEventChainFactory.Create<T>(EventChainFragment<T> fragment) =>
        CreateEventChain(fragment);

    internal IDisposable AddEmitter(object eventId, ITypeInversedUnschedulableEventEmitter eventEmitter)
    {
        lock (_lock) {
            if (!_eventEmitters.TryGetValue(eventId, out var eventEmitters)) {
                eventEmitters = new EventEmitterCollection();
                _eventEmitters[eventId] = eventEmitters;
            }

            return eventEmitters.Add(eventEmitter);
        }
    }

    internal virtual async Task EmitScheduledEventEmissionsAsync<T>(object eventId, T eventData, EventEmissionContext emissionContext)
    {
        foreach (var scheduledEventEmissions in emissionContext.ScheduledEventEmissions) {
            var task = scheduledEventEmissions.Item1(scheduledEventEmissions.Item2);

            if (!task.IsCompletedSuccessfully) {
                await task.ConfigureAwait(false);
            }
        }
    }

    public Task EmitAsync<T>(object eventId, T eventData)
    {
        var context = new EventEmissionContext();

        lock (_lock) {
            if (!_eventEmitters.TryGetValue(eventId, out var eventHandlers)) {
                return Task.CompletedTask;
            }

            eventHandlers.Emit(context, eventData);
        }

        return EmitScheduledEventEmissionsAsync(eventId, eventData, context);
    }

    private class EventEmitterCollection
    {
        private static ulong s_nextId;

        private readonly SortedSet<EventEmitterEntry> _eventEmitterEntries = new();

        public IDisposable Add(ITypeInversedUnschedulableEventEmitter eventHandler)
        {
            var entries = _eventEmitterEntries;
            var entry = new EventEmitterEntry(Interlocked.Increment(ref s_nextId), eventHandler);

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

        public void Emit<T>(EventEmissionContext context, T eventData)
        {
            var entriesCount = _eventEmitterEntries.Count;
            var entries = ArrayPool<EventEmitterEntry>.Shared.Rent(entriesCount);

            try {
                _eventEmitterEntries.CopyTo(entries, 0, entriesCount);

                for (var i = 0; i < entriesCount; i++) {
                    entries[i].Handler.Emit(context, eventData);
                }
            } finally {
                ArrayPool<EventEmitterEntry>.Shared.Return(entries);
            }
        }

        protected readonly record struct EventEmitterEntry : IComparable<EventEmitterEntry>
        {
            public ulong Id { get; }

            public ITypeInversedUnschedulableEventEmitter Handler =>
                _eventHandler ?? throw new InvalidOperationException();

            private readonly ITypeInversedUnschedulableEventEmitter? _eventHandler;

            public EventEmitterEntry(ulong id, ITypeInversedUnschedulableEventEmitter eventHandler)
            {
                Id = id;
                _eventHandler = eventHandler;
            }

            public int CompareTo(EventEmitterEntry other) =>
                Id.CompareTo(other.Id);
        }
    }
}


