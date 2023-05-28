namespace Vernuntii.Reactive.Events;

/// <summary>
/// The implementer can notify observers about emission of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableEvent<out T>
{
    internal bool ContinueSynchronousSubscriptionChaining => false;

    internal IDisposable Subscribe(EventEmissionBacklog emissionBacklog, IEventObserver<T> eventObserver) =>
        throw new IrregularEventSubscriptionException($"The observable event does not support the usage of backlog because {nameof(Subscribe)}({nameof(EventEmissionBacklog)},{nameof(IEventObserver<T>)}) is not implemented");

    /// <summary>
    /// Those who subscribe with <paramref name="eventObserver"/> are able to get notified about new events.
    /// </summary>
    /// <param name="eventObserver"></param>
    /// <returns>
    /// An instance that cancels the subscription if disposed.
    /// </returns>
    IDisposable Subscribe(IEventObserver<T> eventObserver);
}
