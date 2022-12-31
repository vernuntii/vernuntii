namespace Vernuntii.PluginSystem.Reactive;

public sealed record EventChain<T> : IEventChainFactory, IFulfillableEvent<T>
{
    /// <summary>
    /// The event that build on this event chain inherits this registrar.
    /// The disposable of <see cref="Subscribe(IEventFulfiller{T})"/> gets registered to this registrar.
    /// </summary>
    public IDisposableRegistrar? UnsubscriptionRegistrar { get; init; }

    private readonly EventSystem _eventSystem;
    private readonly EventChainFragment<T> _fragment;

    internal EventChain(EventSystem eventSystem, EventChainFragment<T> fragment)
    {
        _eventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
        _fragment = fragment;
    }

    internal EventChain<TNext> Chain<TNext>(EventChainFragment<TNext> fragment) => new(_eventSystem, fragment) {
        UnsubscriptionRegistrar = UnsubscriptionRegistrar
    };

    EventChain<TNext> IEventChainFactory.Create<TNext>(EventChainFragment<TNext> fragment) =>
        Chain(fragment);

    public IDisposable Subscribe(IEventFulfiller<T> eventHandler)
    {
        if (!_fragment.IsEventInitiable) {
            return _fragment.Event.Subscribe(eventHandler);
        }

        var subscription = DelegatingDisposable.Create(
            _fragment.Event.Subscribe(eventHandler).Dispose,
            _eventSystem.AddObserver(_fragment.EventId, new TypeInversedUnschedulableEventObserver<T>(_fragment.EventFulfiller)).Dispose);

        UnsubscriptionRegistrar?.AddDisposable(subscription);
        return subscription;
    }
}
