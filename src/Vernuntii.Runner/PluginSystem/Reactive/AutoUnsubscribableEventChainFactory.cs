namespace Vernuntii.PluginSystem.Reactive;

internal class AutoUnsubscribableEventChainFactory : IEventChainFactory
{
    /// <summary>
    /// The created event chain from this factory will inherit this registrar. The event that build on an event chain inherits the registrar from said event chain.
    /// </summary>
    public IDisposableRegistrar? UnsubscriptionRegistrar { get; init; }

    private readonly IEventChainFactory _eventChainFactory;

    public AutoUnsubscribableEventChainFactory(IEventChainFactory eventChainFactory) =>
        _eventChainFactory = eventChainFactory ?? throw new ArgumentNullException(nameof(eventChainFactory));

    EventChain<T> IEventChainFactory.Create<T>(EventChainFragment<T> fragment) =>
        _eventChainFactory.Create(fragment) with { UnsubscriptionRegistrar = UnsubscriptionRegistrar };
}
