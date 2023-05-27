namespace Vernuntii.Reactive.Centralized;

public interface IDistinguishableEventEmitter
{
    Task EmitAsync<T>(object eventId, T eventData);
}
