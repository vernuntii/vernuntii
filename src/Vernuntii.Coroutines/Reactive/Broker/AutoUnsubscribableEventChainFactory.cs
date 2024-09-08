namespace Vernuntii.Reactive.Broker;

internal class AutoUnsubscribableEventChainFactory : IReadOnlyEventBroker
{
    /// <summary>
    /// The created event chain from this factory will inherit this registrar. The event that build on an event chain inherits the registrar from said event chain.
    /// </summary>
    public IDisposableRegistrar? UnsubscriptionRegistrar { get; init; }

    private readonly IReadOnlyEventBroker _eventChainFactory;

    public AutoUnsubscribableEventChainFactory(IReadOnlyEventBroker eventChainFactory) =>
        _eventChainFactory = eventChainFactory ?? throw new ArgumentNullException(nameof(eventChainFactory));

    EventChain<T> IReadOnlyEventBroker.Chain<T>(EventChainFragment<T> fragment) =>
        _eventChainFactory.Chain(fragment) with { UnsubscriptionRegistrar = UnsubscriptionRegistrar };
}
