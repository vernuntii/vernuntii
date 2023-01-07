namespace Vernuntii.Reactive;

/// <summary>
/// Once every <typeparamref name="TOnce"/> event, the <typeparamref name="TEvery"/>
/// event gets subscribed or the existing subscriptions renewed. After new or renewed subscription,
/// every new <typeparamref name="TEvery"/> event gets emitted.
/// </summary>
/// <typeparam name="TOnce"></typeparam>
/// <typeparam name="TEvery"></typeparam>
internal class OnceEveryEvent<TOnce, TEvery> : EveryEvent<(TOnce, TEvery)>
{
    private readonly IEmittableEvent<TOnce> _once;
    private readonly IEmittableEvent<TEvery> _every;

    public OnceEveryEvent(IEmittableEvent<TOnce> once, IEmittableEvent<TEvery> every)
    {
        _once = once;
        _every = every;
    }

    public override IDisposable Subscribe(IEventEmitter<(TOnce, TEvery)> eventEmitter) =>
        DelegatingDisposable.Create(_ => {
            var emptiableDisposables = new DisposableCollection();

            return new DisposableCollection() {
                emptiableDisposables,
                base.Subscribe(eventEmitter),
                _once.SubscribeUnscheduled(
                (_, once) => {
                    emptiableDisposables.Dispose(permanent: false);

                    emptiableDisposables.TryAdd(
                        () => _every.SubscribeUnscheduled((context, every) => EvaluateEmission(context, (once, every))),
                        out var _);
                })
            }.Dispose;
        });
}
