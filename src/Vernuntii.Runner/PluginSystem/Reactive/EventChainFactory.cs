namespace Vernuntii.PluginSystem.Reactive;

internal static class EventChainFactory
{
    public static EventChain<T> Every<T>(IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();
        return chainFactory.Create(every, every, discriminator.EventId);
    }

    public static EventChain<T> Earliest<T>(IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new EarliestEvent<T>();
        return chainFactory.Create(earliest, earliest, discriminator.EventId);
    }

    public static EventChain<T> OneTime<T>(IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();
        var once = new OneTimeEvent<T>(every);
        return chainFactory.Create(once, every, discriminator.EventId);
    }
}
