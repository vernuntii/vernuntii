using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Broker;

public readonly struct EventId
{
    public object? Value { get; }

    [MemberNotNullWhen(true, nameof(Value))]
    internal bool IsInitialized => Value is not null;

    public EventId(object identity) => Value = identity;
}
