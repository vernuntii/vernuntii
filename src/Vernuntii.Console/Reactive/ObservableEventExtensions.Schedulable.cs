namespace Vernuntii.Reactive;

public static partial class ObservableEventExtensions
{
    public static IDisposable Subscribe<T>(this IObservableEvent<T> @event, Func<T, ValueTask> eventHandler) =>
        @event.Subscribe(new DelegatingEventObserver<T>(eventHandler));

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> @event, Func<T, TState, ValueTask> eventHandler, TState state)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, (Func<T, TState, ValueTask> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);

        return @event.Subscribe(new DelegatingEventObserver<T, (Func<T, TState, ValueTask>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IObservableEvent<T> @event, Func<ValueTask> eventHandler)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, Func<ValueTask>>.Tuple tuple) => tuple.State.Invoke();
        return @event.Subscribe(new DelegatingEventObserver<T, Func<ValueTask>>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> @event, Func<TState, ValueTask> eventHandler, TState state)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, (Func<TState, ValueTask> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.State.State);

        return @event.Subscribe(new DelegatingEventObserver<T, (Func<TState, ValueTask>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IObservableEvent<T> @event, Action<T> eventHandler)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, Action<T>>.Tuple tuple)
        {
            tuple.State.Invoke(tuple.EventData);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, Action<T>>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> @event, Action<T, TState> eventHandler, TState state)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, (Action<T, TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, (Action<T, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IObservableEvent<T> @event, Action eventHandler)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, Action>.Tuple tuple)
        {
            tuple.State.Invoke();
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, Action>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> @event, Action<TState> eventHandler, TState state)
    {
        static ValueTask HandleEvent(in DelegatingEventObserver<T, (Action<TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.State.State);
            return ValueTask.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, (Action<TState>, TState)>(HandleEvent, (eventHandler, state)));
    }
}
