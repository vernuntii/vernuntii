namespace Vernuntii.Reactive.Emissions;

public static partial class ObservableEventExtensions
{
    internal static IDisposable SubscribeBacklogBacked<T>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog, T> eventObserver)
    {
        static void HandleEvent(in DelegatingUnschedulableEventObserver<T, Action<EventEmissionBacklog, T>>.Tuple tuple) =>
            tuple.State.Invoke(tuple.EmissionBacklog, tuple.EventData);

        return observableEvent.Subscribe(new DelegatingUnschedulableEventObserver<T, Action<EventEmissionBacklog, T>>(HandleEvent, eventObserver));
    }

    internal static IDisposable SubscribeBacklogBacked<T, TState>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog, T, TState> eventObserver, TState state)
    {
        static void HandleEvent(in DelegatingUnschedulableEventObserver<T, (Action<EventEmissionBacklog, T, TState> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EmissionBacklog, tuple.EventData, tuple.State.State);

        return observableEvent.Subscribe(new DelegatingUnschedulableEventObserver<T, (Action<EventEmissionBacklog, T, TState>, TState)>(HandleEvent, (eventObserver, state)));
    }

    internal static IDisposable SubscribeBacklogBacked<T>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog> eventObserver)
    {
        static void HandleEvent(in DelegatingUnschedulableEventObserver<T, Action<EventEmissionBacklog>>.Tuple tuple) =>
            tuple.State.Invoke(tuple.EmissionBacklog);

        return observableEvent.Subscribe(new DelegatingUnschedulableEventObserver<T, Action<EventEmissionBacklog>>(HandleEvent, eventObserver));
    }

    internal static IDisposable SubscribeBacklogBacked<T, TState>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog, TState> eventObserver, TState state)
    {
        static void HandleEvent(in DelegatingUnschedulableEventObserver<T, (Action<EventEmissionBacklog, TState> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EmissionBacklog, tuple.State.State);

        return observableEvent.Subscribe(new DelegatingUnschedulableEventObserver<T, (Action<EventEmissionBacklog, TState>, TState)>(HandleEvent, (eventObserver, state)));
    }
}
