namespace Vernuntii.Reactive.Events;

internal interface IBacklogBackedEventObserver<T> : IEventObserver<T>
{
    bool IEventObserver<T>.ContinueBacklogBackedEmissionChaining => true;

    /// <inheritdoc cref="IEventObserver{T}.OnEmission(EventEmissionBacklog, T)"/>
    new void OnEmission(EventEmissionBacklog emissionBacklog, T eventData);

    void IEventObserver<T>.OnEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        OnEmission(emissionBacklog, eventData);

    Task IEventObserver<T>.OnEmissionAsync(T eventData) =>
        throw new NotImplementedException();
}
