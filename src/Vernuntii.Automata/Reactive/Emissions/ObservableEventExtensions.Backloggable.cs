namespace Vernuntii.Reactive.Emissions;

public static partial class ObservableEventExtensions
{
    private static IEventObserver<T> CreateSubscribeBacklogBackedObserver<T>(Action<EventEmissionBacklog, T> eventObserver)
    {
        static void HandleEvent(in DelegatingUnbackloggableEventObserver<T, Action<EventEmissionBacklog, T>>.Tuple tuple) =>
            tuple.State.Invoke(tuple.EmissionBacklog, tuple.EventData);

        return new DelegatingUnbackloggableEventObserver<T, Action<EventEmissionBacklog, T>>(HandleEvent, eventObserver);
    }

    private static IEventObserver<T> CreateSubscribeBacklogBackedObserver<T, TState>(Action<EventEmissionBacklog, T, TState> eventObserver, TState state)
    {
        static void HandleEvent(in DelegatingUnbackloggableEventObserver<T, (Action<EventEmissionBacklog, T, TState> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EmissionBacklog, tuple.EventData, tuple.State.State);

        return new DelegatingUnbackloggableEventObserver<T, (Action<EventEmissionBacklog, T, TState>, TState)>(HandleEvent, (eventObserver, state));
    }

    private static IEventObserver<T> CreateSubscribeBacklogBackedObserver<T>(Action<EventEmissionBacklog> eventObserver)
    {
        static void HandleEvent(in DelegatingUnbackloggableEventObserver<T, Action<EventEmissionBacklog>>.Tuple tuple) =>
            tuple.State.Invoke(tuple.EmissionBacklog);

        return new DelegatingUnbackloggableEventObserver<T, Action<EventEmissionBacklog>>(HandleEvent, eventObserver);
    }

    private static IEventObserver<T> CreateSubscribeBacklogBackedObserver<T, TState>(Action<EventEmissionBacklog, TState> eventObserver, TState state)
    {
        static void HandleEvent(in DelegatingUnbackloggableEventObserver<T, (Action<EventEmissionBacklog, TState> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EmissionBacklog, tuple.State.State);

        return new DelegatingUnbackloggableEventObserver<T, (Action<EventEmissionBacklog, TState>, TState)>(HandleEvent, (eventObserver, state));
    }

    internal static IDisposable SubscribeBacklogBacked<T>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog, T> eventObserver) =>
        observableEvent.Subscribe(CreateSubscribeBacklogBackedObserver(eventObserver));

    internal static IDisposable SubscribeBacklogBacked<T, TState>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog, T, TState> eventObserver, TState state) =>
        observableEvent.Subscribe(CreateSubscribeBacklogBackedObserver(eventObserver, state));

    internal static IDisposable SubscribeBacklogBacked<T>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog> eventObserver) =>
        observableEvent.Subscribe(CreateSubscribeBacklogBackedObserver<T>(eventObserver));

    internal static IDisposable SubscribeBacklogBacked<T, TState>(this IObservableEvent<T> observableEvent, Action<EventEmissionBacklog, TState> eventObserver, TState state) =>
        observableEvent.Subscribe(CreateSubscribeBacklogBackedObserver<T, TState>(eventObserver, state));

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="observableEvent"></param>
    /// <param name="emissionBacklog"></param>
    /// <param name="eventObserver"></param>
    /// <returns></returns>
    private static IDisposable SubscribeBacklogBacked<T>(IObservableEvent<T> observableEvent, EventEmissionBacklog emissionBacklog, IEventObserver<T> eventObserver) =>
        observableEvent.UseEmissionBacklog
        ? observableEvent.Subscribe(emissionBacklog, eventObserver)
        : observableEvent.Subscribe(eventObserver);

    /// <inheritdoc cref="SubscribeBacklogBacked{T}(IObservableEvent{T}, EventEmissionBacklog, IEventObserver{T})"/>
    internal static IDisposable SubscribeBacklogBacked<T>(this IObservableEvent<T> observableEvent, EventEmissionBacklog emissionBacklog, Action<EventEmissionBacklog, T> eventObserver) =>
        SubscribeBacklogBacked(observableEvent, emissionBacklog, CreateSubscribeBacklogBackedObserver(eventObserver));

    /// <inheritdoc cref="SubscribeBacklogBacked{T}(IObservableEvent{T}, EventEmissionBacklog, IEventObserver{T})"/>
    internal static IDisposable SubscribeBacklogBacked<T, TState>(this IObservableEvent<T> observableEvent, EventEmissionBacklog emissionBacklog, Action<EventEmissionBacklog, T, TState> eventObserver, TState state) =>
        SubscribeBacklogBacked(observableEvent, emissionBacklog, CreateSubscribeBacklogBackedObserver(eventObserver, state));

    /// <inheritdoc cref="SubscribeBacklogBacked{T}(IObservableEvent{T}, EventEmissionBacklog, IEventObserver{T})"/>
    internal static IDisposable SubscribeBacklogBacked<T>(this IObservableEvent<T> observableEvent, EventEmissionBacklog emissionBacklog, Action<EventEmissionBacklog> eventObserver) =>
        SubscribeBacklogBacked(observableEvent, emissionBacklog, CreateSubscribeBacklogBackedObserver<T>(eventObserver));

    /// <inheritdoc cref="SubscribeBacklogBacked{T}(IObservableEvent{T}, EventEmissionBacklog, IEventObserver{T})"/>
    internal static IDisposable SubscribeBacklogBacked<T, TState>(this IObservableEvent<T> observableEvent, EventEmissionBacklog emissionBacklog, Action<EventEmissionBacklog, TState> eventObserver, TState state) =>
        SubscribeBacklogBacked(observableEvent, emissionBacklog, CreateSubscribeBacklogBackedObserver<T, TState>(eventObserver, state));
}
