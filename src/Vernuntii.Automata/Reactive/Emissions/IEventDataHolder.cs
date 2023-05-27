using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Emissions;

internal interface IEventDataHolder<T>
{
    [MaybeNull]
    T EventData { get; }

    [MemberNotNullWhen(true, nameof(EventData))]
    bool HasEventData { get; }
}
