using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Reactive;

public static class UniqueEventFulfillerExtensions
{
    public static Task FulfillAsync<T>(this IUniqueEventFulfiller eventFulfiller, IEventDiscriminator<T> discriminator, T eventData) =>
        eventFulfiller.FullfillAsync(discriminator.EventId, eventData);

    public static Task FulfillAsync(this IUniqueEventFulfiller eventFulfiller, IEventDiscriminator<object?> discriminator) =>
        eventFulfiller.FullfillAsync(discriminator.EventId, default(object));
}
