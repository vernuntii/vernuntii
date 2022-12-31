namespace Vernuntii.Reactive;

/// <summary>
/// Represents an event handler, that can be invoked.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventFulfiller<in T>
{
    internal bool IsFulfillmentUnschedulable => false;

    /// <summary>
    /// The method won't be scheduled and gets immediatelly called when the event you subscribed for got fulfilled.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="eventData"></param>
    internal void Fulfill(EventFulfillmentContext context, T eventData) =>
        throw new IrregularEventFulfillmentException("The event fulfillment is schedulable but pretends to be unschedulable");

    /// <summary>
    /// The method gets scheduled and called if the event you subscribed for got fulfilled.
    /// </summary>
    /// <param name="eventData"></param>
    Task OnFulfilled(T eventData);
}
