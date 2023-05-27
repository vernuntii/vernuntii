namespace Vernuntii.Reactive.Disposables;

internal class DelegatingDisposable<TState, TConstructorState> : DelegatingDisposable<TState>
{
    internal DelegatingDisposable(
        Func<IDisposableLifetime, TConstructorState, (Action<TState> DisposeHandler, TState State)> argumentsFactory,
        TConstructorState constructorState) =>
        (_disposeHandler, _state) = argumentsFactory(this, constructorState);
}
