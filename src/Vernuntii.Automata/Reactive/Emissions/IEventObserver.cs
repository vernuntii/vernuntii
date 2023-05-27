namespace Vernuntii.Reactive.Emissions;

/// <summary>
/// An interface with the capability to accept emissions.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventObserver<in T>
{
    internal bool UseEmissionBacklog => false;

    /// <summary>
    /// The method won't be scheduled and gets immediatelly called when the event you subscribed for gets emitted.
    /// </summary>
    /// <param name="emissionBacklog"></param>
    /// <param name="eventData"></param>
    internal void OnEmission(EventEmissionBacklog emissionBacklog, T eventData) =>
        throw new IrregularEventEmissionException($"The event observer does not support the usage of backlog because {nameof(OnEmission)} is not implemented");

    /// <summary>
    /// If the observer does use the emission backlog directly, then the call to this method gets added to the emissionBacklog of current emission emissionBacklog.
    /// After the synchronous emission chain reached its end the asynchronous emission emissionBacklog will be processed one by one.
    /// </summary>
    /// <param name="eventData"></param>
    Task OnEmissionAsync(T eventData);
}
