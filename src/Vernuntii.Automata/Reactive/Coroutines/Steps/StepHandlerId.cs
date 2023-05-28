using System.Diagnostics;

namespace Vernuntii.Reactive.Coroutines.Steps;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct StepHandlerId
{
    private object? _value { get; }

    [MemberNotNullWhen(true, nameof(_value))]
    internal bool IsInitialized => _value is not null;

    public StepHandlerId(object identity) => _value = identity;

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is StepHandlerId otherHandlerId
        && (ReferenceEquals(_value, otherHandlerId._value)
            || Equals(_value, otherHandlerId._value));

    public override int GetHashCode() =>
        _value?.GetHashCode() ?? 0;

    private string? GetDebuggerDisplay() =>
        _value?.ToString() ?? ToString();
}
