namespace Vernuntii.PluginSystem.Reactive;

public sealed class EventChain<T> : IEventChainFactory, IObservableEvent<T>
{
    EventSystem IEventChainFactory.EventSystem =>
        _eventSystem;

    private readonly EventSystem _eventSystem;
    private readonly EventChainFragment<T> _fragment;

    internal EventChain(EventSystem eventSystem, EventChainFragment<T> fragment)
    {
        _eventSystem = eventSystem ?? throw new ArgumentNullException(nameof(eventSystem));
        _fragment = fragment;
    }

    internal EventChain<TNext> Chain<TNext>(EventChainFragment<TNext> fragment) =>
        new(_eventSystem, fragment);

    public IDisposable Subscribe(IEventObserver<T> eventHandler)
    {
        if (!_fragment.HasEventId) {
            return _fragment.Event.Subscribe(eventHandler);
        }

        return DelegatingDisposable.Create(
            _fragment.Event.Subscribe(eventHandler).Dispose,
            _eventSystem.AddObserver(_fragment.EventId.Value, new EventObserver<T>(_fragment.EventInitiator)).Dispose);
    }
}
