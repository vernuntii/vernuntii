namespace Vernuntii.Reactive;

/// <summary>
/// Represents an event handler, that can be invoked.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventObserver<in T>
{
    internal bool IsUnschedulable => false;

    /// <summary>
    /// The method won't be scheduled and gets immediatelly called when the event you subscribed for got fulfilled.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="eventData"></param>
    internal void OnFulfilled(EventFulfillmentContext context, T eventData) =>
        throw new NotImplementedException();

    /// <summary>
    /// The method gets scheduled and called if the event you subscribed for got fulfilled.
    /// </summary>
    /// <param name="eventData"></param>
    Task OnFulfilled(T eventData);
}
