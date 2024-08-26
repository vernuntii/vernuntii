using Vernuntii.Coroutines;
using Vernuntii.Reactive.Broker;

namespace Vernuntii.Reactive.Extensions.Coroutines;

partial class EffectExtensions
{
    public static Coroutine<EventChannel<T>> Channel<T>(this Effect _, Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector)
    {
        var completionSource = ValueTaskCompletionSource<EventChannel<T>>.RentFromCache();
        return new Coroutine<EventChannel<T>>(completionSource.CreateGenericValueTask(), ArgumentReceiverDelegate);

        void ArgumentReceiverDelegate(ref CoroutineArgumentReceiver argumentReceiver)
        {
            var argument = new Arguments.ObserveArgument<T>(eventSelector, completionSource);
            argumentReceiver.ReceiveCallableArgument(in argument, in Arguments.ObserveArgumentType);
        }
    }

    partial class Arguments
    {
        internal readonly struct ObserveArgument<T>(
            Func<IReadOnlyEventBroker, IObservableEvent<T>> eventSelector,
            ValueTaskCompletionSource<EventChannel<T>> completionSource) : ICallableArgument
        {
            private readonly ValueTaskCompletionSource<EventChannel<T>> _completionSource = completionSource;

            ICoroutineCompletionSource ICallableArgument.CompletionSource => _completionSource;

            void ICallableArgument.Callback(in CoroutineContext coroutineContext) => throw new NotImplementedException();
        }
    }
}
