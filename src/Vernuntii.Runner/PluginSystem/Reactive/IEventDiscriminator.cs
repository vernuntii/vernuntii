namespace Vernuntii.PluginSystem.Reactive;

public interface IEventDiscriminator<TPayload>
{
    /// <summary>
    /// The event id.
    /// </summary>
    object EventId { get; }
}
