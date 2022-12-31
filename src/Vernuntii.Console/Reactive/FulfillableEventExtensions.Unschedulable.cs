namespace Vernuntii.Reactive;

public static partial class FulfillableEventExtensions
{
    internal static IDisposable SubscribeUnscheduled<T>(this IFulfillableEvent<T> @event, Action<EventFulfillmentContext, T> eventHandler)
    {
        static ValueTask HandleEvent(in DelegatingUnschedulableEventObserver<T, Action<EventFulfillmentContext, T>>.Tuple tuple)
        {
            tuple.State.Invoke(tuple.FulfillmentContext, tuple.EventData);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingUnschedulableEventObserver<T, Action<EventFulfillmentContext, T>>(HandleEvent, eventHandler));
    }

    internal static IDisposable SubscribeUnscheduled<T, TState>(this IFulfillableEvent<T> @event, Action<EventFulfillmentContext, T, TState> eventHandler, TState state)
    {
        static ValueTask HandleEvent(in DelegatingUnschedulableEventObserver<T, (Action<EventFulfillmentContext, T, TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.FulfillmentContext, tuple.EventData, tuple.State.State);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingUnschedulableEventObserver<T, (Action<EventFulfillmentContext, T, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }

    internal static IDisposable SubscribeUnscheduled<T>(this IFulfillableEvent<T> @event, Action<EventFulfillmentContext> eventHandler)
    {
        static ValueTask HandleEvent(in DelegatingUnschedulableEventObserver<T, Action<EventFulfillmentContext>>.Tuple tuple)
        {
            tuple.State.Invoke(tuple.FulfillmentContext);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingUnschedulableEventObserver<T, Action<EventFulfillmentContext>>(HandleEvent, eventHandler));
    }

    internal static IDisposable SubscribeUnscheduled<T, TState>(this IFulfillableEvent<T> @event, Action<EventFulfillmentContext, TState> eventHandler, TState state)
    {
        static ValueTask HandleEvent(in DelegatingUnschedulableEventObserver<T, (Action<EventFulfillmentContext, TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.FulfillmentContext, tuple.State.State);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingUnschedulableEventObserver<T, (Action<EventFulfillmentContext, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }
}
