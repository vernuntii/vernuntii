using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem.Reactive;

internal static class EventChainFragment
{
    public static EventChainFragment<T> Create<T>(IFulfillableEvent<T> @event, IUnschedulableEventFulfiller<T> eventFulfiller, object eventId) =>
        new(@event, eventFulfiller, eventId);

    public static EventChainFragment<T> Create<T>(IFulfillableEvent<T> @event) =>
        new(@event);
}

internal record EventChainFragment<T>
{
    internal IFulfillableEvent<T> Event { get; }

    internal IUnschedulableEventFulfiller<T>? EventFulfiller =>
        _eventInitiator ?? throw new InvalidOperationException();

    internal object? EventId { get; }

    [MemberNotNullWhen(true,
        nameof(EventId),
        nameof(EventFulfiller))]
    internal bool IsEventInitiable => EventId != null;

    private readonly IUnschedulableEventFulfiller<T>? _eventInitiator;

    public EventChainFragment(IFulfillableEvent<T> @event, IUnschedulableEventFulfiller<T> eventFulfiller, object eventId)
    {
        Event = @event;
        _eventInitiator = eventFulfiller;
        EventId = eventId;
    }

    public EventChainFragment(IFulfillableEvent<T> @event) =>
        Event = @event;
}
