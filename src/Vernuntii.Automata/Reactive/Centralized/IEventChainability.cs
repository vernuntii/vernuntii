namespace Vernuntii.Reactive.Centralized;

public interface IEventChainability
{
    internal EventChain<T> Chain<T>(EventChainFragment<T> fragment);
}
