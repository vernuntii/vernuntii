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

    public static EventChain<T> Every<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator) =>
        EventChainFactory.Every(chainFactory, discriminator);

    public static EventChain<(TOnce, TEvery)> OnceEveryThen<TOnce, TEvery>(this IEventChainFactory chainFactory, EventChain<TOnce> once, EventChain<TEvery> then) =>
        chainFactory.Create(new OnceEveryThenEvent<TOnce, TEvery>(once, then));

    public static EventChain<(TOnce, TEvery)> OnceEveryThenEvery<TOnce, TEvery>(
        this IEventChainFactory chainFactory,
        EventChain<TOnce> once,
        IEventDiscriminator<TEvery> everyDiscriminator) =>
        chainFactory.OnceEveryThen(
            once,
            chainFactory.Every(everyDiscriminator));

    public static EventChain<(TOnce, TEvery)> OnceEveryThenEvery<TOnce, TEvery>(
        this IEventChainFactory chainFactory,
        IEventDiscriminator<TOnce> onceDiscriminator,
        EventChain<TEvery> every) =>
        chainFactory.OnceEveryThen(
            chainFactory.Every(onceDiscriminator),
            every);

    public static EventChain<(TOnce, TEvery)> OnceEveryThenEvery<TOnce, TEvery>(
        this IEventChainFactory chainFactory,
        IEventDiscriminator<TOnce> onceDiscriminator,
        IEventDiscriminator<TEvery> everyDiscriminator) =>
        chainFactory.OnceEveryThen(
            chainFactory.Every(onceDiscriminator),
            chainFactory.Every(everyDiscriminator));

    public static EventChain<(TOnce, TFirst)> OnceEveryXReplayOneTimeXY<TOnce, TFirst>(this IEventChainFactory chainFactory, EventChain<TOnce> oneTimeX, EventChain<TFirst> oneTimeY) =>
        chainFactory.Create(new OnceEveryXReplayOneTimeXYEvent<TOnce, TFirst>(oneTimeX, oneTimeY));

    public static EventChain<(TOnce, TFirst)> OnceEveryXReplayOneTimeXY<TOnce, TFirst>(
        this IEventChainFactory chainFactory,
        EventChain<TOnce> oneTimeY,
        IEventDiscriminator<TFirst> oneTimeXDiscriminator) =>
        chainFactory.OnceEveryXReplayOneTimeXY(
            oneTimeY,
            chainFactory.Every(oneTimeXDiscriminator));

    public static EventChain<(TOnce, TFirst)> OnceEveryXReplayOneTimeXY<TOnce, TFirst>(
        this IEventChainFactory chainFactory,
        IEventDiscriminator<TOnce> oneTimeXDiscriminator,
        EventChain<TFirst> oneTimeY) =>
        chainFactory.OnceEveryXReplayOneTimeXY(
            chainFactory.Every(oneTimeXDiscriminator),
            oneTimeY);

    public static EventChain<(TOnce, TFirst)> OnceEveryXReplayOneTimeXY<TOnce, TFirst>(
        this IEventChainFactory chainFactory,
        IEventDiscriminator<TOnce> oneTimeXDiscriminator,
        IEventDiscriminator<TFirst> oneTimeYDiscriminator) =>
        chainFactory.OnceEveryXReplayOneTimeXY(
            chainFactory.Every(oneTimeXDiscriminator),
            chainFactory.Every(oneTimeYDiscriminator));

    public static EventChain<T> Earliest<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator) =>
        EventChainFactory.Earliest(chainFactory, discriminator);

    public static EventChain<T> OneTime<T>(this IEventChainFactory chainFactory, IEventDiscriminator<T> discriminator) =>
        EventChainFactory.OneTime(chainFactory, discriminator);
}
