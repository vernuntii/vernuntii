namespace Vernuntii.Reactive;

internal interface IUnschedulableEventEmitter<T> : IEventEmitter<T>
{
    bool IEventEmitter<T>.IsEmissionUnschedulable => true;

    /// <inheritdoc cref="IEventEmitter{T}.Emit(EventEmissionContext, T)"/>
    new void Emit(EventEmissionContext context, T eventData);

    void IEventEmitter<T>.Emit(EventEmissionContext context, T eventData) =>
        Emit(context, eventData);

    Task IEventEmitter<T>.EmitAsync(T eventData) =>
        throw new NotImplementedException();
}
