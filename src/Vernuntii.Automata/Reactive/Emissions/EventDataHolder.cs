using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Emissions;

internal class EventDataHolder<T> : IEventDataHolder<T>
{
    [AllowNull, MaybeNull]
    public T EventData { get; }

    [MemberNotNullWhen(true, nameof(EventData))]
    public bool HasEventData { get; }

    public EventDataHolder(T eventData, bool hasEventData)
    {
        EventData = eventData;
        HasEventData = hasEventData;
    }
}
