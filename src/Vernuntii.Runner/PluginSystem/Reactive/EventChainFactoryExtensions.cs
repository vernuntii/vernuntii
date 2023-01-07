namespace Vernuntii.PluginSystem.Reactive;

/// <summary>
/// Extension methods for <see cref="IEventChainFactory"/>.
/// </summary>
public static class EventChainFactoryExtensions
{
    /// <summary>
    /// The event chains that are created with the help of this returning factory will inherit <paramref name="unsubscriptionRegistrar"/>.
    /// The event that build on an event chain inherits the registrar from said event chain.
    /// </summary>
    /// <param name="chainFactory"></param>
    /// <param name="unsubscriptionRegistrar">The bequeathable unsubscription registar. Specifiying <see langword="null"/> breaks the inheritance for new event chains.</param>
    /// <returns>
    /// A new instance with the capability to bequeath <paramref name="unsubscriptionRegistrar"/> to created event chains.
    /// </returns>
    public static IEventChainFactory UseUnsubscriptionRegistrar(this IEventChainFactory chainFactory, IDisposableRegistrar? unsubscriptionRegistrar) =>
        new AutoUnsubscribableEventChainFactory(chainFactory) { UnsubscriptionRegistrar = unsubscriptionRegistrar };

    internal static EventChain<T> Create<T>(this IEventChainFactory chainFactory, IEmittableEvent<T> @event, IUnschedulableEventEmitter<T> eventEmitter, object eventId) =>
        chainFactory.Create(EventChainFragment.Create(@event, eventEmitter, eventId));

    internal static EventChain<T> Create<T>(this IEventChainFactory chainFactory, IEmittableEvent<T> @event) =>
        chainFactory.Create(EventChainFragment.Create(@event));

    public static EventChain<T> Every<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var every = new EveryEvent<T>();
        return chainFactory.Create(every, every, discriminator.EventId);
    }

    public static EventChain<(TOnce, TEvery)> OnceEvery<TOnce, TEvery>(this IEventChainFactory chainFactory, EventChain<TOnce> once, EventChain<TEvery> every) =>
        chainFactory.Create(new OnceEveryEvent<TOnce, TEvery>(once, every));

    public static EventChain<(TOnce, TEvery)> OnceEvery<TOnce, TEvery>(
        this IEventChainFactory chainFactory,
        EventChain<TOnce> once,
        IEventDiscriminator<TEvery> everyDiscriminator) =>
        chainFactory.OnceEvery(
            once,
            chainFactory.Every(everyDiscriminator));

    public static EventChain<(TOnce, TEvery)> OnceEvery<TOnce, TEvery>(
        this IEventChainFactory chainFactory,
        IEventDiscriminator<TOnce> onceDiscriminator,
        EventChain<TEvery> every) =>
        chainFactory.OnceEvery(
            chainFactory.Every(onceDiscriminator),
            every);

    public static EventChain<(TOnce, TEvery)> OnceEvery<TOnce, TEvery>(
        this IEventChainFactory chainFactory,
        IEventDiscriminator<TOnce> onceDiscriminator,
        IEventDiscriminator<TEvery> everyDiscriminator) =>
        chainFactory.OnceEvery(
            chainFactory.Every(onceDiscriminator),
            chainFactory.Every(everyDiscriminator));

    public static EventChain<T> Earliest<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator)
    {
        var earliest = new EarliestEvent<T>();
        return chainFactory.Create(earliest, earliest, discriminator.EventId);
    }
}
