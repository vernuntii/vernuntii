namespace Vernuntii.PluginSystem.Events;

public record EventDiscriminator<TPayload> : IEventDiscriminator<TPayload>
{
    /// <inheritdoc/>
    public ulong EventId { get; }

    /// <summary>
    /// Creates a copy of this type.
    /// </summary>
    /// <param name="original"></param>
    public EventDiscriminator(EventDiscriminator<TPayload> original) =>
        EventId = original.EventId;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    public EventDiscriminator() =>
        EventId = EventIdentifier.Next();
}
