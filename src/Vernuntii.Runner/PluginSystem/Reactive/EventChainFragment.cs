using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem.Reactive;

internal static class EventChainFragment
{
    public static EventChainFragment<T> Create<T>(IEmittableEvent<T> @event, IUnschedulableEventEmitter<T> eventEmitter, object eventId) =>
        new(@event, eventEmitter, eventId);

    public static EventChainFragment<T> Create<T>(IEmittableEvent<T> @event) =>
        new(@event);
}

internal record EventChainFragment<T>
{
    internal IEmittableEvent<T> Event { get; }

    internal IUnschedulableEventEmitter<T>? EventEmitter =>
        _eventEmitter ?? throw new InvalidOperationException();

    internal object? EventId { get; }

    [MemberNotNullWhen(true,
        nameof(EventId),
        nameof(EventEmitter))]
    internal bool IsEventIdentifiable => EventId != null;

    private readonly IUnschedulableEventEmitter<T>? _eventEmitter;

    public EventChainFragment(IEmittableEvent<T> @event, IUnschedulableEventEmitter<T> eventEmitter, object eventId)
    {
        Event = @event;
        _eventEmitter = eventEmitter;
        EventId = eventId;
    }

    public EventChainFragment(IEmittableEvent<T> @event) =>
        Event = @event;
}
