using System.Runtime.CompilerServices;

namespace Vernuntii.PluginSystem.Reactive;

/// <inheritdoc/>
public record EventDiscriminator : EventDiscriminator<object?>
{
    public static EventDiscriminator<TPayload> New<TPayload>([CallerMemberName] string? eventName = null) =>
        new EventDiscriminator<TPayload>() { EventName = eventName };

    public static EventDiscriminator New([CallerMemberName] string? eventName = null) =>
        new EventDiscriminator() { EventName = eventName };

    protected EventDiscriminator()
    {
    }
}
