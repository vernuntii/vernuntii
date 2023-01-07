namespace Vernuntii.PluginSystem.Reactive;

/// <summary>
/// Represents an event. It has chain-semantic behaviour which means,
/// that it bequeath data and behaviour to events that are originating
/// from this instance. This event chain is immutable and can be changed
/// by creating a new instance of it with the <see langword="with"/>-keyword.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record EventChain<T> : IEventChainFactory, IEmittableEvent<T>
{
    /// <summary>
    /// The event that build on this event chain inherits this registrar.
    /// The disposable of <see cref="Subscribe(IEventEmitter{T})"/> gets registered to this registrar.
    /// </summary>
    public IDisposableRegistrar? UnsubscriptionRegistrar { get; init; }

    private readonly EventSystem _eventSystem;
    internal readonly EventChainFragment<T> _fragment;

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

    /// <inheritdoc/>
    public IDisposable Subscribe(IEventEmitter<T> eventHandler)
    {
        if (!_fragment.IsEventIdentifiable) {
            return _fragment.Event.Subscribe(eventHandler);
        }

        var subscription = DelegatingDisposable.Create(lifetime => {
            return DelegatingDisposable.Create(
                _eventSystem.AddEmitter(_fragment.EventId, new TypeInversedUnschedulableEventEmitter<T>(_fragment.EventEmitter)).Dispose,
                _fragment.Event.SubscribeUnscheduled(static (context, eventData, state) => {
                    var (eventHandler, lifetime) = state;
                    context.MakeOrScheduleEventEmission(eventHandler, eventData);

                    if (context.IsCompleting) {
                        lifetime.Dispose();
                    }
                }, (eventHandler, lifetime))
                .Dispose)
            .Dispose;
        });

        // TODO: Make revocable
        UnsubscriptionRegistrar?.AddDisposable(subscription);
        return subscription;
    }
}
