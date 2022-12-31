namespace Vernuntii.Reactive;

internal static class DelegatingDisposable
{
    public static DelegatingDisposable<T> Create<T>(Action<T> disposeHandler, T state) =>
        new(disposeHandler, state);

    public static DelegatingDisposable<Action[]> Create(params Action[] disposables) => Create(
        static state => {
            foreach (var disposable in state) {
                disposable();
            }
        },
        disposables);
}
