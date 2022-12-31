namespace Vernuntii.Reactive;

internal interface IUnschedulableEventFulfiller<T> : IEventFulfiller<T>
{
    bool IEventFulfiller<T>.IsFulfillmentUnschedulable => true;

    /// <inheritdoc cref="IEventFulfiller{T}.Fulfill(EventFulfillmentContext, T)"/>
    new void Fulfill(EventFulfillmentContext context, T eventData);

    void IEventFulfiller<T>.Fulfill(EventFulfillmentContext context, T eventData) =>
        Fulfill(context, eventData);

    Task IEventFulfiller<T>.OnFulfilled(T eventData) => throw new NotImplementedException();
}
