namespace Vernuntii.Reactive.Broker;

public interface IEventChainability
{
    internal EventChain<T> Chain<T>(EventChainFragment<T> fragment);
}
