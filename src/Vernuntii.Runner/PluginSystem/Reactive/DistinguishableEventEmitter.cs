namespace Vernuntii.PluginSystem.Reactive;

public static class DistinguishableEventEmitter
{
    public static Task EmitAsync<T>(this IDistinguishableEventEmitter eventEmitter, IEventDiscriminator<T> discriminator, T eventData) =>
        eventEmitter.EmitAsync(discriminator.EventId, eventData);

    public static Task EmitAsync(this IDistinguishableEventEmitter eventEmitter, IEventDiscriminator<object?> discriminator) =>
        eventEmitter.EmitAsync(discriminator.EventId, default(object));
}
