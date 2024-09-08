namespace Vernuntii.Reactive.Broker;

public class EventChainTemplate<T>
{
    private EventChain<T>? _eventChain;
    private IEventDiscriminator<T>? _eventDiscriminator;
    private readonly Func<IReadOnlyEventBroker, IEventDiscriminator<T>, EventChain<T>>? _eventChainCreator;

    [MemberNotNullWhen(true, nameof(_eventChain))]
    [MemberNotNullWhen(false, nameof(_eventDiscriminator), nameof(_eventChainCreator))]
    protected bool HasEventChain => _eventChain is not null;

    internal EventChainTemplate(IEventDiscriminator<T> eventDiscriminator, Func<IReadOnlyEventBroker, IEventDiscriminator<T>, EventChain<T>> eventChainCreator)
    {
        _eventDiscriminator = eventDiscriminator;
        _eventChainCreator = eventChainCreator;
    }

    internal EventChainTemplate(EventChain<T>? eventChain) => _eventChain = eventChain;

    internal EventChain<T> GetOrCreateChain(IReadOnlyEventBroker eventChainability)
    {
        if (HasEventChain) {
            return _eventChain;
        }

        return _eventChainCreator(eventChainability, _eventDiscriminator);
    }

    public static implicit operator EventChainTemplate<T>(EventChain<T> eventChain) =>
        new EventChainTemplate<T>(eventChain);
}
