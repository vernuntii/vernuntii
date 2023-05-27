namespace Vernuntii.Reactive.Emissions;

/// <summary>
/// Once every <typeparamref name="TWhenever"/> event, the <typeparamref name="TResubscribe"/>
/// event gets subscribed or the existing subscriptions renewed. After new or renewed subscription,
/// every new <typeparamref name="TResubscribe"/> event gets emitted.
/// </summary>
/// <typeparam name="TWhenever"></typeparam>
/// <typeparam name="TResubscribe"></typeparam>
internal class WheneverThenResubscribeEvent<TWhenever, TResubscribe> : EveryEvent<(TWhenever, TResubscribe)>
{
    private readonly IObservableEvent<TWhenever> _whenever;
    private readonly IObservableEvent<TResubscribe> _resubscribe;

    public WheneverThenResubscribeEvent(IObservableEvent<TWhenever> whenever, IObservableEvent<TResubscribe> resubscribe)
    {
        _whenever = whenever;
        _resubscribe = resubscribe;
    }

    public override IDisposable Subscribe(IEventObserver<(TWhenever, TResubscribe)> eventObserver) =>
        DelegatingDisposable.Create(_ => {
            var resubscribeDisposables = new DisposableCollection();

            return new DisposableCollection() {
                resubscribeDisposables,
                base.Subscribe(eventObserver),
                _whenever.SubscribeBacklogBacked(
                (_, whenever) => {
                    resubscribeDisposables.Dispose(permanently: false);

                    resubscribeDisposables.TryAdd(
                        () => _resubscribe.SubscribeBacklogBacked((emissionBacklog, resubscribe) => EvaluateEmission(emissionBacklog, (whenever, resubscribe))),
                        out var _);
                })
            }.Dispose;
        });
}
