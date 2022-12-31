namespace Vernuntii.Reactive;

public static partial class FulfillableEventExtensions
{
    public static IDisposable Subscribe<T>(this IFulfillableEvent<T> @event, Func<T, Task> eventHandler) =>
        @event.Subscribe(new DelegatingEventObserver<T>(eventHandler));

    public static IDisposable Subscribe<T, TState>(this IFulfillableEvent<T> @event, Func<T, TState, Task> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Func<T, TState, Task> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);

        return @event.Subscribe(new DelegatingEventObserver<T, (Func<T, TState, Task>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IFulfillableEvent<T> @event, Func<Task> eventHandler)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, Func<Task>>.Tuple tuple) => tuple.State.Invoke();
        return @event.Subscribe(new DelegatingEventObserver<T, Func<Task>>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IFulfillableEvent<T> @event, Func<TState, Task> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Func<TState, Task> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.State.State);

        return @event.Subscribe(new DelegatingEventObserver<T, (Func<TState, Task>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IFulfillableEvent<T> @event, Action<T> eventHandler)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, Action<T>>.Tuple tuple)
        {
            tuple.State.Invoke(tuple.EventData);
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, Action<T>>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IFulfillableEvent<T> @event, Action<T, TState> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Action<T, TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, (Action<T, TState>, TState)>(HandleEvent, (eventHandler, state)));
    }

    public static IDisposable Subscribe<T>(this IFulfillableEvent<T> @event, Action eventHandler)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, Action>.Tuple tuple)
        {
            tuple.State.Invoke();
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, Action>(HandleEvent, eventHandler));
    }

    public static IDisposable Subscribe<T, TState>(this IFulfillableEvent<T> @event, Action<TState> eventHandler, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Action<TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.State.State);
            return Task.CompletedTask;
        }

        return @event.Subscribe(new DelegatingEventObserver<T, (Action<TState>, TState)>(HandleEvent, (eventHandler, state)));
    }
}
