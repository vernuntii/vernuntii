namespace Vernuntii.PluginSystem.Events;

public record EventDiscriminator<TPayload> : IEventDiscriminator<TPayload>
{
    /// <inheritdoc/>
    public ulong EventId { get; }

    /// <inheritdoc/>
    public string? EventName { get; init; }

    private IComparable? _cachedEventId;

    object IEventDiscriminator<TPayload>.EventId =>
        GetEventId();

    /// <summary>
    /// Creates a copy of this type.
    /// </summary>
    /// <param name="original"></param>
    public EventDiscriminator(EventDiscriminator<TPayload> original)
    {
        EventId = original.EventId;
        _cachedEventId = original._cachedEventId;
    }

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    public EventDiscriminator() =>
        EventId = EventIdentifier.Next();

    /// <summary>
    /// Gets the event id.
    /// </summary>
    /// <returns>Either <see cref="EventId"/>, or if <see cref="EventName"/> is null or wrhite-space its wrapped version that emits <see cref="EventName"/> on <see cref="object.ToString"/></returns>
    public virtual IComparable GetEventId() => _cachedEventId ??= (string.IsNullOrWhiteSpace(EventName)
        ? EventId
        : new NamedEventId(this));

    private class NamedEventId : IComparable<NamedEventId>, IComparable<ulong>, IComparable, IEquatable<NamedEventId>
    {
        private readonly EventDiscriminator<TPayload> _eventDiscriminator;

        public NamedEventId(EventDiscriminator<TPayload> eventDiscriminator) =>
            _eventDiscriminator = eventDiscriminator;

        public int CompareTo(ulong other) =>
            _eventDiscriminator.EventId.CompareTo(other);

        public int CompareTo(NamedEventId? other) => other is null
            ? -1
            : CompareTo(other._eventDiscriminator.EventId);

        public int CompareTo(object? obj)
        {
            if (obj is null) {
                return -1;
            }

            if (obj is NamedEventId namedEvent) {
                return CompareTo(namedEvent);
            }

            if (obj is ulong eventId) {
                return CompareTo(eventId);
            }

            throw new ArgumentException($"The object you want to compare against must be either of type {nameof(NamedEventId)} or {nameof(UInt64)}");
        }

        private bool IsEquivalentTo(ulong other) =>
            _eventDiscriminator.EventId == other;

        private bool IsEquivalentTo(NamedEventId other) =>
            IsEquivalentTo(other._eventDiscriminator.EventId);

        public bool Equals(NamedEventId? other) =>
            other is not null && IsEquivalentTo(other);

        public override bool Equals(object? obj) =>
            (obj is NamedEventId otherNamedEventId && IsEquivalentTo(otherNamedEventId))
            || obj is ulong otherEventId && IsEquivalentTo(otherEventId);

        public override string ToString() =>
            _eventDiscriminator.EventName ?? _eventDiscriminator.EventId.ToString();
    }
}
