namespace Vernuntii.PluginSystem.Reactive;

public static class DistinguishableEventFulfiller
{
    public static Task FulfillAsync<T>(this IDistinguishableEventFulfiller eventFulfiller, IEventDiscriminator<T> discriminator, T eventData) =>
        eventFulfiller.FullfillAsync(discriminator.EventId, eventData);

    public static Task FulfillAsync(this IDistinguishableEventFulfiller eventFulfiller, IEventDiscriminator<object?> discriminator) =>
        eventFulfiller.FullfillAsync(discriminator.EventId, default(object));
}
