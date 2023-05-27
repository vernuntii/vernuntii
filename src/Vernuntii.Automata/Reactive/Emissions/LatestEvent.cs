using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Emissions;

internal class LatestEvent<T> : EveryEvent<T>, IEventDataHolder<T>
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

    [AllowNull, MaybeNull]
    private T _eventData;

    private bool _hasEventData;

    protected override IEventDataHolder<T>? InspectEmission(T eventData)
    {
        _eventData = eventData;
        _hasEventData = true;
        return this;
    }
}
