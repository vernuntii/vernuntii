namespace Vernuntii.Reactive.Emissions;

internal class WhereEvent<TSource, TState> : IObservableEvent<TSource>
{
    private readonly IObservableEvent<TSource> _source;
    private readonly Func<TSource, TState, bool> _predicate;
    private readonly TState _predicateState;

    public WhereEvent(IObservableEvent<TSource> source, Func<TSource, TState, bool> predicate, TState predicateState)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _predicateState = predicateState;
    }

    public IDisposable Subscribe(IEventObserver<TSource> eventObserver) =>
        _source.SubscribeBacklogBacked((emissionBacklog, eventData) => {
            if (!_predicate(eventData, _predicateState)) {
                return;
            }

            emissionBacklog.EmitOrBacklog(eventObserver, eventData);
        });
}
