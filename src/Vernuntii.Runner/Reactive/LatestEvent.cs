using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive;

internal class LatestEvent<T> : EveryEvent<T>
{
    [MaybeNull]
    internal override T EventData => _eventData;

    [MemberNotNullWhen(true, nameof(EventData))]
    internal override bool HasEventData => _hasEventData;

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
