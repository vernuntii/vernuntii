using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Events;

internal interface IEventDataHolder<T>
{
    [MaybeNull]
    T EventData { get; }

    [MemberNotNullWhen(true, nameof(EventData))]
    bool HasEventData { get; }
}
