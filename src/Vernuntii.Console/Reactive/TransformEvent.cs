namespace Vernuntii.Reactive;

internal class TransformEvent<TSource, TResult> : IObservableEvent<TResult>
{
    private readonly IObservableEvent<TSource> _source;
    private readonly Func<TSource, TResult> _transformHandler;

    public TransformEvent(IObservableEvent<TSource> source, Func<TSource, TResult> transformHandler)
    {
        _source = source;
        _transformHandler = transformHandler ?? throw new ArgumentNullException(nameof(transformHandler));
    }

    public IDisposable Subscribe(IEventObserver<TResult> observer) =>
        DelegatingDisposable.Create(
            _source.SubscribeUnscheduled(
                static (context, eventData, state) => state.observer.OnFulfilled(context, state.transform(eventData)),
                (observer, transform: _transformHandler)).Dispose);
}
