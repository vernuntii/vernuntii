using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.PluginSystem.Reactive;

internal static class EventChainFragment
{
    public static EventChainFragment<T> Create<T>(IObservableEvent<T> @event, IUnschedulableEventObserver<T> eventInitiator, ulong eventId) =>
        new(@event, eventInitiator, eventId);

    public static EventChainFragment<T> Create<T>(IObservableEvent<T> @event) =>
        new(@event);
}

internal record EventChainFragment<T>
{
    internal IObservableEvent<T> Event { get; }

    internal IUnschedulableEventObserver<T>? EventInitiator =>
        _eventInitiator ?? throw new InvalidOperationException();

    internal ulong? EventId { get; }

    [MemberNotNullWhen(true,
        nameof(EventId),
        nameof(EventInitiator))]
    internal bool HasEventId => EventId != null;

    private readonly IUnschedulableEventObserver<T>? _eventInitiator;

    public EventChainFragment(IObservableEvent<T> @event, IUnschedulableEventObserver<T> eventInitiator, ulong eventId)
    {
        Event = @event;
        _eventInitiator = eventInitiator;
        EventId = eventId;
    }

    public EventChainFragment(IObservableEvent<T> @event) =>
        Event = @event;
}
