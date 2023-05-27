namespace Vernuntii.Reactive;

/// <summary>
/// Once every <typeparamref name="TOneTimeX"/> event, the <typeparamref name="TOneTimeX"/> and <typeparamref name="TOneTimeY"/>
/// are emitted, if <typeparamref name="TOneTimeX"/> and <typeparamref name="TOneTimeY"/> have emitted once.
/// </summary>
/// <typeparam name="TOneTimeX"></typeparam>
/// <typeparam name="TOneTimeY"></typeparam>
internal class OnceEveryXReplayOneTimeXYEvent<TOneTimeX, TOneTimeY> : EveryEvent<(TOneTimeX, TOneTimeY)>
{
    private readonly IEmittableEvent<TOneTimeX> _oneTimeX;
    private readonly IEmittableEvent<TOneTimeY> _oneTimeY;
    private MutableEventDataHolder<TOneTimeY> _oneTimeYEventDataHolder;

    public OnceEveryXReplayOneTimeXYEvent(IEmittableEvent<TOneTimeX> oneTimeX, IEmittableEvent<TOneTimeY> oneTimeY)
    {
        _oneTimeX = oneTimeX ?? throw new ArgumentNullException(nameof(oneTimeX));
        _oneTimeY = oneTimeY ?? throw new ArgumentNullException(nameof(oneTimeY));
        _oneTimeYEventDataHolder = new();
    }

    private bool TryEvaluateEmission(EventEmissionContext context, TOneTimeX oneTimeX)
    {
        if (!_oneTimeYEventDataHolder.HasEventData) {
            return false;
        }

        EvaluateEmission(context, (oneTimeX, _oneTimeYEventDataHolder.EventData));
        return true;
    }

    public override IDisposable Subscribe(IEventEmitter<(TOneTimeX, TOneTimeY)> eventEmitter) =>
        DelegatingDisposable.Create(_ => {
            var oneTimeYDisposables = new DisposableCollection();
            var oneTimeXEventData = default(MutableEventDataHolder<TOneTimeX>);

            var onceEveryDisposables = DisposableCollection.Of(
                oneTimeYDisposables,
                base.Subscribe(eventEmitter),
                _oneTimeX.SubscribeUnscheduled((context, oneTimeX) => {
                    if (TryEvaluateEmission(context, oneTimeX)) {
                        return;
                    }

                    if (oneTimeXEventData == null) {
                        oneTimeXEventData = new MutableEventDataHolder<TOneTimeX>();
                        oneTimeXEventData.EventData = oneTimeX;
                        oneTimeXEventData.HasEventData = true;
                    }
                }));

            var oneTimeYSubscription = _oneTimeY.SubscribeUnscheduled((context, oneTimeY) => {
                oneTimeYDisposables.Dispose();

                if (!_oneTimeYEventDataHolder.HasEventData) {
                    _oneTimeYEventDataHolder.EventData = oneTimeY;
                    _oneTimeYEventDataHolder.HasEventData = true;
                }

                if (oneTimeXEventData?.HasEventData == true) {
                    oneTimeXEventData.HasEventData = false;
                    EvaluateEmission(context, (oneTimeXEventData.EventData, oneTimeY));
                }
            });

            oneTimeYDisposables.TryAdd(oneTimeYSubscription);
            return onceEveryDisposables.Dispose;
        });
}
