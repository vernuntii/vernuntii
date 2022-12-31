namespace Vernuntii.PluginSystem.Reactive;

public interface IEventChainFactory
{
    internal EventChain<T> Create<T>(EventChainFragment<T> fragment);
}
