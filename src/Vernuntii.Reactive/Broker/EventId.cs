using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.Reactive.Broker;

public readonly struct EventId(object identity)
{
    public object? Value { get; } = identity;

    [MemberNotNullWhen(true, nameof(Value))]
    internal bool IsInitialized => Value is not null;
}
