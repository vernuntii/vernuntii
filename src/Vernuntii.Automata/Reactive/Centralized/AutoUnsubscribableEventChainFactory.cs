namespace Vernuntii.Reactive.Centralized;

internal class AutoUnsubscribableEventChainFactory : IEventChainability
{
    /// <summary>
    /// The created event chain from this factory will inherit this registrar. The event that build on an event chain inherits the registrar from said event chain.
    /// </summary>
    public IDisposableRegistrar? UnsubscriptionRegistrar { get; init; }

    private readonly IEventChainability _eventChainFactory;

    public AutoUnsubscribableEventChainFactory(IEventChainability eventChainFactory) =>
        _eventChainFactory = eventChainFactory ?? throw new ArgumentNullException(nameof(eventChainFactory));

    EventChain<T> IEventChainability.Chain<T>(EventChainFragment<T> fragment) =>
        _eventChainFactory.Chain(fragment) with { UnsubscriptionRegistrar = UnsubscriptionRegistrar };
}
