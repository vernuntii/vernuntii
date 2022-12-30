namespace Vernuntii.PluginSystem.Events;

public interface IEventDiscriminator<TPayload>
{
    /// <summary>
    /// The event id.
    /// </summary>
    object EventId { get; }
}
