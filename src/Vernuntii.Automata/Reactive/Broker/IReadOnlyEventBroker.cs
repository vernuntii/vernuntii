namespace Vernuntii.Reactive.Broker;

public interface IReadOnlyEventBroker
{
    internal EventChain<T> Chain<T>(EventChainFragment<T> fragment);
}
