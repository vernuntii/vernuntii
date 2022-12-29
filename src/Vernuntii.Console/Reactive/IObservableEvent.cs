namespace Vernuntii.Reactive;

/// <summary>
/// The implementer can produce instances of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IObservableEvent<out T>
{
    /// <summary>
    /// Those who subscribe with <paramref name="eventHandler"/> are able to get notified about new events.
    /// </summary>
    /// <param name="eventHandler"></param>
    /// <returns>
    /// An instance that cancels the subscription if disposed.
    /// </returns>
    IDisposable Subscribe(IEventObserver<T> eventHandler);
}
