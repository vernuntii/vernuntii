namespace Vernuntii.Reactive;

/// <summary>
/// Once every <typeparamref name="TOnce"/> event, the <typeparamref name="TFirst"/>
/// event gets subscribed or the existing subscriptions renewed. After new or renewed subscription,
/// every new <typeparamref name="TFirst"/> event gets emitted.
/// </summary>
/// <typeparam name="TOnce"></typeparam>
/// <typeparam name="TFirst"></typeparam>
internal class OnceFirstEvent<TOnce, TFirst> : EveryEvent<(TOnce, TFirst)>
{
    private readonly IEmittableEvent<TOnce> _once;
    private readonly IEmittableEvent<TFirst> _first;
    private MutableEventDataHolder<TFirst> _firstEventDataHolder;

    public OnceFirstEvent(IEmittableEvent<TOnce> once, IEmittableEvent<TFirst> first)
    {
        _once = once ?? throw new ArgumentNullException(nameof(once));
        _first = first ?? throw new ArgumentNullException(nameof(first));
        _firstEventDataHolder = new();
    }

    private void EvaluateEmission(EventEmissionContext context, TOnce once, TFirst first)
    {
        lock (_firstEventDataHolder) {
            if (_firstEventDataHolder.HasEventData) {
                return;
            }

            _firstEventDataHolder.EventData = first;
            _firstEventDataHolder.HasEventData = true;
        }

        EvaluateEmission(context, (once, first));
    }

    private bool TryEvaluateEmission(EventEmissionContext context, TOnce once)
    {
        lock (_firstEventDataHolder) {
            if (!_firstEventDataHolder.HasEventData) {
                return false;
            }
        }

        EvaluateEmission(context, (once, _firstEventDataHolder.EventData));
        return true;
    }

    public override IDisposable Subscribe(IEventEmitter<(TOnce, TFirst)> eventEmitter) =>
        DelegatingDisposable.Create(_ => {
            var emptiableDisposables = new DisposableCollection();

            return new DisposableCollection() {
                emptiableDisposables,
                base.Subscribe(eventEmitter),
                _once.SubscribeUnscheduled(
                (context, once) => {
                    emptiableDisposables.Dispose(permanently: false);

                    if (TryEvaluateEmission(context, once)) {
                        return;
                    }

                    emptiableDisposables.TryAdd(
                        () => _first.SubscribeUnscheduled((context, every) => EvaluateEmission(context, once, every)),
                        out var _);
                })
            }.Dispose;
        });
}
