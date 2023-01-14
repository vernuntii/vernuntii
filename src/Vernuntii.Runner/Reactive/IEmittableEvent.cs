namespace Vernuntii.Reactive;

/// <summary>
/// The implementer can produce instances of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEmittableEvent<out T>
{
    /// <summary>
    /// Those who subscribe with <paramref name="eventEmitter"/> are able to get notified about new events.
    /// </summary>
    /// <param name="eventEmitter"></param>
    /// <returns>
    /// An instance that cancels the subscription if disposed.
    /// </returns>
    IDisposable Subscribe(IEventEmitter<T> eventEmitter);
}
