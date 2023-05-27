namespace Vernuntii.Reactive.Centralized;

public static class DistinguishableEventObserverExtensions
{
    public static Task EmitAsync<T>(this IDistinguishableEventEmitter eventEmitter, IEventDiscriminator<T> eventDiscriminator, T eventData) =>
        eventEmitter.EmitAsync(eventDiscriminator.EventId, eventData);

    public static Task EmitAsync(this IDistinguishableEventEmitter eventEmitter, IEventDiscriminator<object?> eventDiscriminator) =>
        eventEmitter.EmitAsync(eventDiscriminator.EventId, default(object));
}
