namespace Vernuntii.Reactive.Centralized;

public interface IEventDiscriminator<TPayload>
{
    /// <summary>
    /// The event id.
    /// </summary>
    EventId EventId { get; }
}
