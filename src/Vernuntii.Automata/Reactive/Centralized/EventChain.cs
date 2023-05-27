namespace Vernuntii.Reactive.Centralized;

/// <summary>
/// Represents an event. It has chain-semantic behaviour which means,
/// that it bequeath data and behaviour to events that are originating
/// from this instance. This event chain is immutable and can be changed
/// by creating a new instance of it with the <see langword="with"/>-keyword.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record EventChain<T> : IEventChainability, IObservableEvent<T>
{
    /// <summary>
    /// The event that build on this event chain inherits this registrar.
    /// The disposable of <see cref="Subscribe(IEventObserver{T})"/> gets registered to this registrar.
    /// </summary>
    public IDisposableRegistrar? UnsubscriptionRegistrar { get; init; }

    private readonly EventStore _eventStore;
    internal readonly EventChainFragment<T> _fragment;

    internal EventChain(EventStore eventStore, EventChainFragment<T> fragment)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _fragment = fragment;
    }

    internal EventChain<TNext> Chain<TNext>(EventChainFragment<TNext> fragment) => new(_eventStore, fragment) {
        UnsubscriptionRegistrar = UnsubscriptionRegistrar
    };

    EventChain<TNext> IEventChainability.Chain<TNext>(EventChainFragment<TNext> fragment) =>
        Chain(fragment);

    /// <inheritdoc/>
    public IDisposable Subscribe(IEventObserver<T> eventObserver)
    {
        if (!_fragment.IsEventAllowingBridging) {
            return _fragment.Event.Subscribe(eventObserver);
        }

        var subscription = DelegatingDisposable.Create(lifetime => {
            return DelegatingDisposable.Create(
                _eventStore.AddObserver(_fragment.EventId, new TypeInversedChainableEventObserver<T>(_fragment.EventObserver)).Dispose,
                _fragment.Event.SubscribeBacklogBacked(static (emissionBacklog, eventData, state) => {
                    var (eventObserver, lifetime) = state;
                    eventObserver.OnEmissionOrBacklog(emissionBacklog, eventData);
                }, (eventObserver, lifetime)).Dispose)
            .Dispose;
        });

        // TODO: Make revocable
        UnsubscriptionRegistrar?.AddDisposable(subscription);
        return subscription;
    }
}
