namespace Vernuntii.Reactive;

public static partial class EmittableEventExtensions
{
    internal static IDisposable SubscribeUnscheduled<T>(this IEmittableEvent<T> @event, Action<EventEmissionContext, T> eventHandler)
    {
        static void HandleEvent(in DelegatingUnschedulableEventEmitter<T, Action<EventEmissionContext, T>>.Tuple tuple) =>
            tuple.State.Invoke(tuple.EmissionContext, tuple.EventData);

        return @event.Subscribe(new DelegatingUnschedulableEventEmitter<T, Action<EventEmissionContext, T>>(HandleEvent, eventHandler));
    }

    internal static IDisposable SubscribeUnscheduled<T, TState>(this IEmittableEvent<T> @event, Action<EventEmissionContext, T, TState> eventHandler, TState state)
    {
        static void HandleEvent(in DelegatingUnschedulableEventEmitter<T, (Action<EventEmissionContext, T, TState> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EmissionContext, tuple.EventData, tuple.State.State);

        return @event.Subscribe(new DelegatingUnschedulableEventEmitter<T, (Action<EventEmissionContext, T, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }

    internal static IDisposable SubscribeUnscheduled<T>(this IEmittableEvent<T> @event, Action<EventEmissionContext> eventHandler)
    {
        static void HandleEvent(in DelegatingUnschedulableEventEmitter<T, Action<EventEmissionContext>>.Tuple tuple) =>
            tuple.State.Invoke(tuple.EmissionContext);

        return @event.Subscribe(new DelegatingUnschedulableEventEmitter<T, Action<EventEmissionContext>>(HandleEvent, eventHandler));
    }

    internal static IDisposable SubscribeUnscheduled<T, TState>(this IEmittableEvent<T> @event, Action<EventEmissionContext, TState> eventHandler, TState state)
    {
        static void HandleEvent(in DelegatingUnschedulableEventEmitter<T, (Action<EventEmissionContext, TState> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EmissionContext, tuple.State.State);

        return @event.Subscribe(new DelegatingUnschedulableEventEmitter<T, (Action<EventEmissionContext, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }
}
