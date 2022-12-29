using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Reactive;

public static class EventChainFactoryExtensions
{
    public static EventChain<T> Every<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();

        return new EventChain<T>(
            chainFactory.EventSystem,
            EventChainFragment.Create(every, every, discriminator.EventId));
    }

    public static EventChain<T> Earliest<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new EarliestEvent<T>();

        return new EventChain<T>(
            chainFactory.EventSystem,
            EventChainFragment.Create(earliest, earliest, discriminator.EventId));
    }
}
