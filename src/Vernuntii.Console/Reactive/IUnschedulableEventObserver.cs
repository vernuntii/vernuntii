namespace Vernuntii.Reactive;

internal interface IUnschedulableEventObserver<T> : IEventObserver<T>
{
    bool IEventObserver<T>.IsUnschedulable => true;

    /// <inheritdoc cref="IEventObserver{T}.OnFulfilled(EventFulfillmentContext, T)"/>
    new void OnFulfilled(EventFulfillmentContext context, T eventData);

    void IEventObserver<T>.OnFulfilled(EventFulfillmentContext context, T eventData) =>
        OnFulfilled(context, eventData);

    ValueTask IEventObserver<T>.OnFulfilled(T eventData) => throw new NotImplementedException();
}
