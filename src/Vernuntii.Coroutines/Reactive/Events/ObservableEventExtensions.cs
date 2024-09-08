namespace Vernuntii.Reactive.Events;

public static partial class ObservableEventExtensions
{
    public static IDisposable Subscribe<T>(this IObservableEvent<T> observableEvent, Func<T, Task> eventObserver) =>
        observableEvent.Subscribe(new DelegatingEventObserver<T>(eventObserver));

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> observableEvent, Func<T, TState, Task> eventObserver, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Func<T, TState, Task> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);

        return observableEvent.Subscribe(new DelegatingEventObserver<T, (Func<T, TState, Task>, TState)>(HandleEvent, (eventObserver, state)));
    }

    public static IDisposable Subscribe<T>(this IObservableEvent<T> observableEvent, Func<Task> eventObserver)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, Func<Task>>.Tuple tuple) => tuple.State.Invoke();
        return observableEvent.Subscribe(new DelegatingEventObserver<T, Func<Task>>(HandleEvent, eventObserver));
    }

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> observableEvent, Func<TState, Task> eventObserver, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Func<TState, Task> EventHandler, TState State)>.Tuple tuple) =>
            tuple.State.EventHandler.Invoke(tuple.State.State);

        return observableEvent.Subscribe(new DelegatingEventObserver<T, (Func<TState, Task>, TState)>(HandleEvent, (eventObserver, state)));
    }

    public static IDisposable Subscribe<T>(this IObservableEvent<T> observableEvent, Action<T> eventObserver)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, Action<T>>.Tuple tuple)
        {
            tuple.State.Invoke(tuple.EventData);
            return Task.CompletedTask;
        }

        return observableEvent.Subscribe(new DelegatingEventObserver<T, Action<T>>(HandleEvent, eventObserver));
    }

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> observableEvent, Action<T, TState> eventObserver, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Action<T, TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.EventData, tuple.State.State);
            return Task.CompletedTask;
        }

        return observableEvent.Subscribe(new DelegatingEventObserver<T, (Action<T, TState>, TState)>(HandleEvent, (eventObserver, state)));
    }

    public static IDisposable Subscribe<T>(this IObservableEvent<T> observableEvent, Action eventObserver)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, Action>.Tuple tuple)
        {
            tuple.State.Invoke();
            return Task.CompletedTask;
        }

        return observableEvent.Subscribe(new DelegatingEventObserver<T, Action>(HandleEvent, eventObserver));
    }

    public static IDisposable Subscribe<T, TState>(this IObservableEvent<T> observableEvent, Action<TState> eventObserver, TState state)
    {
        static Task HandleEvent(in DelegatingEventObserver<T, (Action<TState> EventHandler, TState State)>.Tuple tuple)
        {
            tuple.State.EventHandler.Invoke(tuple.State.State);
            return Task.CompletedTask;
        }

        return observableEvent.Subscribe(new DelegatingEventObserver<T, (Action<TState>, TState)>(HandleEvent, (eventObserver, state)));
    }
}
