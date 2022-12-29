﻿using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Reactive;

public static class EventSystemExtensions
{
    public static ValueTask FulfillAsync<T>(this EventSystem eventSystem, IEventDiscriminator<T> discriminator, T eventData) =>
        eventSystem.FullfillAsync(discriminator.EventId, eventData);

    public static ValueTask FulfillAsync(this EventSystem eventSystem, IEventDiscriminator<object?> discriminator) =>
        eventSystem.FullfillAsync(discriminator.EventId, default(object));
}
