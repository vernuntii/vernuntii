namespace Vernuntii.Reactive.Centralized;

internal interface ITypeInversedChainableEventObserver
{
    void OnEmission<T>(EventEmissionBacklog emissionBacklog, T eventData);
}
