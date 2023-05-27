namespace Vernuntii.Reactive.Centralized;

public interface IDistinguishableEventEmitter
{
    Task EmitAsync<T>(EventId eventId, T eventData);
}
