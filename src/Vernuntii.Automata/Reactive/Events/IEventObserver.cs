namespace Vernuntii.Reactive.Events;

/// <summary>
/// An interface with the capability to observe emission.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventObserver<in T>
{
    internal bool ContinueSynchronousEmissionChaining => false;

    /// <summary>
    /// The method won't be scheduled and gets immediatelly called when the event you subscribed for gets emitted.
    /// </summary>
    /// <param name="emissionBacklog"></param>
    /// <param name="eventData"></param>
    internal void OnEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        throw new IrregularEventEmissionException($"The event observer does not support the usage of emission backlog because {nameof(OnEmission)}({nameof(EventEmissionBacklog)},{nameof(T)}) is not implemented");

    /// <summary>
    /// If the observer does use the emission backlog directly, then the call to this method gets added to the emission backlog of current emission backlog.
    /// After the synchronous emission chain reached its end the asynchronous emission backlog will be processed one by one.
    /// </summary>
    /// <param name="eventData"></param>
    Task OnEmissionAsync(T eventData);
}
