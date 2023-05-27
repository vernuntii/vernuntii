namespace Vernuntii.Reactive.Emissions;

/// <summary>
/// The implementer can notify about instances of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableEvent<out T>
{
    /// <summary>
    /// Those who subscribe with <paramref name="eventObserver"/> are able to get notified about new events.
    /// </summary>
    /// <param name="eventObserver"></param>
    /// <returns>
    /// An instance that cancels the subscription if disposed.
    /// </returns>
    IDisposable Subscribe(IEventObserver<T> eventObserver);
}
