namespace Vernuntii.Reactive;

/// <summary>
/// Represents an event handler, that can be invoked.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEventEmitter<in T>
{
    internal bool IsEmissionUnschedulable => false;

    /// <summary>
    /// The method won't be scheduled and gets immediatelly called when the event you subscribed for got emitted.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="eventData"></param>
    internal void Emit(EventEmissionContext context, T eventData) =>
        throw new IrregularEventEmissionException("The event fulfillment is schedulable but pretends to be unschedulable");

    /// <summary>
    /// The method gets scheduled and called if the event you subscribed for got emitted.
    /// </summary>
    /// <param name="eventData"></param>
    Task EmitAsync(T eventData);
}
