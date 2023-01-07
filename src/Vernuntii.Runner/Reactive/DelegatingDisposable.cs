namespace Vernuntii.Reactive;

internal static class DelegatingDisposable
{
    public static IDelegatingDisposable Create<T>(Action<T> disposeHandler, T state) =>
        new DelegatingDisposable<T>(disposeHandler, state);

    public static IDelegatingDisposable Create(Action disposeHandler) =>
        Create(static disposeHandler => disposeHandler(), disposeHandler);

    internal static IDelegatingDisposable Create<T>(Func<IDisposableLifetime, (Action<T> DisposeHandler, T State)> argumentsFactory) =>
        new DelegatingDisposable<T>(argumentsFactory);

    public static IDelegatingDisposable Create(params Action[] disposables) => Create(
        static disposables => {
            foreach (var disposable in disposables) {
                disposable();
            }
        },
        disposables);

    internal static IDelegatingDisposable Create<TState, TConstructorState>(
        Func<IDisposableLifetime, TConstructorState, (Action<TState> DisposeHandler, TState State)> argumentsFactory,
        TConstructorState constructorState) =>
        new DelegatingDisposable<TState, TConstructorState>(argumentsFactory, constructorState);

    internal static IDelegatingDisposable Create<TConstructorState>(
        Func<IDisposableLifetime, TConstructorState, Action> argumentsFactory,
        TConstructorState constructorState) =>
        new DelegatingDisposable<Action, (Func<IDisposableLifetime, TConstructorState, Action> ArgumentsFactory, TConstructorState ConstructorState)>(
            static (instance, constructorState) => (static disposeHandler => disposeHandler(), constructorState.ArgumentsFactory(instance, constructorState.ConstructorState)),
            (argumentsFactory, constructorState));

    internal static IDelegatingDisposable Create(Func<IDisposableLifetime, Action> argumentsFactory) =>
        new DelegatingDisposable<Action, Func<IDisposableLifetime, Action>>(
            static (instance, argumentsFactory) => (static disposeHandler => disposeHandler(), argumentsFactory(instance)), argumentsFactory);
}
