namespace Vernuntii.Reactive;

public static partial class EmittableEventExtensions
{
    public static IDisposable Subscribe<T>(this IEmittableEvent<T> @event, Func<T, Task> eventHandler) =>
        @event.Subscribe(new DelegatingEventEmitter<T>(eventHandler));

    public static IDisposable Subscribe<T, TState>(this IEmittableEvent<T> @event, Func<T, TState, Task> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, (Func<T, TState, Task> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);

        return @event.Subscribe(new DelegatingEventEmitter<T, (Func<T, TState, Task>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IEmittableEvent<T> @event, Func<Task> eventHandler)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, Func<Task>>.Tuple tuple) => tuple.State.Invoke();
        return @event.Subscribe(new DelegatingEventEmitter<T, Func<Task>>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IEmittableEvent<T> @event, Func<TState, Task> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, (Func<TState, Task> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.State.State);

        return @event.Subscribe(new DelegatingEventEmitter<T, (Func<TState, Task>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IEmittableEvent<T> @event, Action<T> eventHandler)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, Action<T>>.Tuple tuple)
        {
            tuple.State.Invoke(tuple.EventData);
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventEmitter<T, Action<T>>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IEmittableEvent<T> @event, Action<T, TState> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, (Action<T, TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventEmitter<T, (Action<T, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IEmittableEvent<T> @event, Action eventHandler)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, Action>.Tuple tuple)
        {
            tuple.State.Invoke();
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventEmitter<T, Action>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IEmittableEvent<T> @event, Action<TState> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventEmitter<T, (Action<TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.State.State);
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventEmitter<T, (Action<TState>, TState)>(HandleEvent, (eventHandler, state)));
    }
}
