namespace Vernuntii.PluginSystem.Reactive;

public interface IDistinguishableEventEmitter
{
    Task EmitAsync<T>(object eventId, T eventData);
}
