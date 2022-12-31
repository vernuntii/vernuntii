using Vernuntii.PluginSystem.Events;

namespace Vernuntii.PluginSystem.Reactive;

/// <summary>
/// Extension methods for <see cref="IEventChainFactory"/>.
/// </summary>
public static class EventChainFactoryExtensions
{
    /// <summary>
    /// The event chains that are created with the help of returning factory will inherit <paramref name="unsubscriptionRegistrar"/>.
    /// The event that build on an event chain inherits the registrar from said event chain.
    /// </summary>
    /// <param name="unsubscriptionRegistrar">The bequeathable unsubscription registar. Specifiying <see langword="null"/> breaks the inheritance for new event chains.</param>
    /// <returns>
    /// A new instance with the capability to bequeath <paramref name="unsubscriptionRegistrar"/> to created event chains.
    /// </returns>
    public static IEventChainFactory AutoUnsubscribeEvents(this IEventChainFactory chainFactory, IDisposableRegistrar? unsubscriptionRegistrar) =>
        new AutoUnsubscribableEventChainFactory(chainFactory) { UnsubscriptionRegistrar = unsubscriptionRegistrar };

    internal static EventChain<T> Create<T>(this IEventChainFactory chainFactory, IFulfillableEvent<T> @event, IUnschedulableEventFulfiller<T> eventFulfiller, object eventId) =>
        chainFactory.Create(EventChainFragment.Create(@event, eventFulfiller, eventId));

    public static EventChain<T> Every<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();
        return chainFactory.Create(every, every, discriminator.EventId);
    }

    public static EventChain<T> Earliest<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new EarliestEvent<T>();
        return chainFactory.Create(earliest, earliest, discriminator.EventId);
    }
}
