using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Events;

internal class LatestEvent<T> : Event<T>, IEventDataHolder<T>, IObservableEvent<T>
{
    [MaybeNull]
    internal virtual T EventData => _eventData;

    [MemberNotNullWhen(true, nameof(EventData))]
    internal virtual bool HasEventData => _hasEventData;

    [MaybeNull]
    T IEventDataHolder<T>.EventData =>
        EventData;

    bool IEventDataHolder<T>.HasEventData =>
        HasEventData;

    bool IObservableEvent<T>.ContinueSynchronousSubscriptionChaining => true;

    [AllowNull, MaybeNull]
    private T _eventData;

    private bool _hasEventData;

    protected override IEventDataHolder<T>? InspectEmission(T eventData)
    {
        _eventData = eventData;
        _hasEventData = true;
        return this;
    }

    IDisposable IObservableEvent<T>.Subscribe(EventEmissionBacklog emissionBacklog, IEventObserver<T> eventObserver)
    {
        if (HasEventData) {
            eventObserver.OnEmissionOrBacklog(emissionBacklog, EventData);
        }

        return Subscribe(eventObserver);
    }
}
