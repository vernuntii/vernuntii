namespace Vernuntii.Reactive.Centralized;

public static class EventChainabilities
{
    public static EventChain<T> Every<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();
        return chainFactory.Chain(every, every, discriminator.EventId);
    }

    public static EventChain<T> Earliest<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new EarliestEvent<T>();
        return chainFactory.Chain(earliest, earliest, discriminator.EventId);
    }

    public static EventChain<T> One<T>(IEventChainability chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();
        var one = new OneEvent<T>(every);
        return chainFactory.Chain(one, every, discriminator.EventId);
    }
}
