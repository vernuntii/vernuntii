namespace Vernuntii.Reactive.Broker;

public static class EventChainabilities
{
    public static EventChain<T> Every<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new Event<T>();
        return chainFactory.Chain(every, every, discriminator.EventId);
    }

    public static EventChain<T> Latest<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new LatestEvent<T>();
        return chainFactory.Chain(earliest, earliest, discriminator.EventId);
    }

    public static EventChain<T> Earliest<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new EarliestEvent<T>();
        return chainFactory.Chain(earliest, earliest, discriminator.EventId);
    }

    public static EventChain<T> One<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new Event<T>();
        var one = new OneEvent<T>(every);
        return chainFactory.Chain(one, every, discriminator.EventId);
    }
}
