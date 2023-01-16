namespace Vernuntii.Reactive;

/// <summary>
/// Once every <typeparamref name="TOnceEvery"/> event, the <typeparamref name="TFirst"/>
/// event gets subscribed or the existing subscriptions renewed. After new or renewed subscription,
/// every new <typeparamref name="TFirst"/> event gets emitted.
/// </summary>
/// <typeparam name="TOnceEvery"></typeparam>
/// <typeparam name="TFirst"></typeparam>
internal class OnceEveryReplayFirstEvent<TOnceEvery, TFirst> : EveryEvent<(TOnceEvery, TFirst)>
{
    private readonly IEmittableEvent<TOnceEvery> _onceEvery;
    private readonly IEmittableEvent<TFirst> _first;
    private MutableEventDataHolder<TFirst> _firstEventDataHolder;

    public OnceEveryReplayFirstEvent(IEmittableEvent<TOnceEvery> onceEvery, IEmittableEvent<TFirst> first)
    {
        _onceEvery = onceEvery ?? throw new ArgumentNullException(nameof(onceEvery));
        _first = first ?? throw new ArgumentNullException(nameof(first));
        _firstEventDataHolder = new();
    }

    private bool TryEvaluateEmission(EventEmissionContext context, TOnceEvery once)
    {
        if (!_firstEventDataHolder.HasEventData) {
            return false;
        }

        EvaluateEmission(context, (once, _firstEventDataHolder.EventData));
        return true;
    }

    public override IDisposable Subscribe(IEventEmitter<(TOnceEvery, TFirst)> eventEmitter) =>
        DelegatingDisposable.Create(_ => {
            var firstDisposables = new DisposableCollection();
            var onceEveryEventData = default(MutableEventDataHolder<TOnceEvery>);

            var onceEveryDisposables = DisposableCollection.Of(
                firstDisposables,
                base.Subscribe(eventEmitter),
                _onceEvery.SubscribeUnscheduled((context, onceEvery) => {
                    if (TryEvaluateEmission(context, onceEvery)) {
                        return;
                    }

                    if (onceEveryEventData == null) {
                        onceEveryEventData = new MutableEventDataHolder<TOnceEvery>();
                        onceEveryEventData.EventData = onceEvery;
                        onceEveryEventData.HasEventData = true;
                    }
                }));

            var firstSubscription = _first.SubscribeUnscheduled((context, first) => {
                firstDisposables.Dispose();

                if (!_firstEventDataHolder.HasEventData) {
                    _firstEventDataHolder.EventData = first;
                    _firstEventDataHolder.HasEventData = true;
                }

                if (onceEveryEventData?.HasEventData == true) {
                    onceEveryEventData.HasEventData = false;
                    EvaluateEmission(context, (onceEveryEventData.EventData, first));
                }
            });

            firstDisposables.TryAdd(firstSubscription);
            return onceEveryDisposables.Dispose;
        });
}
